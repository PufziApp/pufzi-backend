
using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pufzi.Contracts.Requests.Bookings;
using Pufzi.Contracts.Responses.Bookings;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.Bookings;

public partial class BookingService
{
    public async Task<BookingChangeRequestResponse> CreateBookingChangeRequestAsync(
        Guid userId,
        Guid bookingId,
        CreateBookingChangeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || bookingId == Guid.Empty)
            throw new ArgumentException("Utilizatorul sau rezervarea este invalidă.");

        if (request.Appointments is null ||
            request.Appointments.Count is < 1 or > 10)
            throw new ArgumentException(
                "Selectează între 1 și 10 programări pentru modificare.");

        if (request.Reason?.Length > 2000)
            throw new ArgumentException("Motivul este prea lung.");

        var appointmentIds = request.Appointments
            .Select(x => x.AppointmentId)
            .ToList();

        if (appointmentIds.Contains(Guid.Empty) ||
            appointmentIds.Distinct().Count() != appointmentIds.Count)
            throw new ArgumentException(
                "Programările selectate sunt invalide sau duplicate.");

        var nowUtc = DateTime.UtcNow;

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

        var booking = await _dbContext.Bookings
            .FromSqlInterpolated(
                $"""SELECT * FROM "Bookings" WHERE "Id" = {bookingId} FOR UPDATE""")
            .SingleOrDefaultAsync(cancellationToken);

        if (booking is null)
            throw new KeyNotFoundException("Rezervarea nu a fost găsită.");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException(
                "Nu poți modifica rezervarea altui client.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException(
                "Doar rezervările confirmate pot fi modificate.");

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.BookingId == bookingId)
            .ToListAsync(cancellationToken);

        if (appointments.Count == 0)
            throw new InvalidOperationException(
                "Rezervarea nu conține programări.");

        if (appointments.Any(x => x.StartAt <= nowUtc))
            throw new InvalidOperationException(
                "Nu poți modifica o rezervare care a început deja.");

        var existingPending = await _dbContext.BookingChangeRequests
            .AnyAsync(x =>
                x.BookingId == bookingId &&
                x.Status == BookingChangeRequestStatus.Pending &&
                (!x.ExpiresAt.HasValue || x.ExpiresAt > nowUtc),
                cancellationToken);

        if (existingPending)
            throw new InvalidOperationException(
                "Există deja o solicitare de modificare în așteptare.");

        var appointmentsById = appointments.ToDictionary(x => x.Id);

        var changeRequest = new BookingChangeRequest
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            RequestedByUserId = userId,
            Status = BookingChangeRequestStatus.Pending,
            Reason = request.Reason?.Trim(),
            ExpiresAt = new[]
            {
                nowUtc.AddHours(24),
                appointments.Min(x => x.StartAt)
            }.Min(),
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };

        var response = new BookingChangeRequestResponse
        {
            Id = changeRequest.Id,
            BookingId = bookingId,
            RequestedByUserId = userId,
            Status = changeRequest.Status.ToString(),
            Reason = changeRequest.Reason,
            ExpiresAt = changeRequest.ExpiresAt,
            CreatedAt = nowUtc
        };

        foreach (var input in request.Appointments)
        {
            if (!appointmentsById.TryGetValue(
                input.AppointmentId,
                out var appointment))
                throw new ArgumentException(
                    "Una dintre programări nu aparține rezervării.");

            if (input.ProposedStartAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException(
                    "Ora propusă trebuie transmisă în UTC.");

            if (input.ProposedStartAtUtc <= nowUtc)
                throw new ArgumentException(
                    "Nu poți propune o programare în trecut.");

            if (input.ProposedStartAtUtc == appointment.StartAt)
                throw new ArgumentException(
                    "Ora propusă trebuie să fie diferită de cea actuală.");

            var proposedEnd = input.ProposedStartAtUtc
                .AddMinutes(appointment.DurationMinutes);

            changeRequest.Appointments.Add(
                new BookingChangeAppointment
                {
                    Id = Guid.NewGuid(),
                    BookingChangeRequestId = changeRequest.Id,
                    AppointmentId = appointment.Id,
                    ProposedStartAt = input.ProposedStartAtUtc,
                    ProposedEndAt = proposedEnd
                });

            response.Appointments.Add(
                new BookingChangeAppointmentResponse
                {
                    AppointmentId = appointment.Id,
                    ProposedStartAtUtc = input.ProposedStartAtUtc,
                    ProposedEndAtUtc = proposedEnd
                });
        }

        var proposedTimes = appointments
            .Select(x =>
            {
                var change = changeRequest.Appointments
                    .FirstOrDefault(c => c.AppointmentId == x.Id);

                return new
                {
                    Start = change?.ProposedStartAt ?? x.StartAt,
                    End = change?.ProposedEndAt ?? x.EndAt
                };
            })
            .OrderBy(x => x.Start)
            .ToList();

        for (var i = 1; i < proposedTimes.Count; i++)
        {
            if (proposedTimes[i].Start < proposedTimes[i - 1].End)
                throw new ArgumentException(
                    "Orele propuse se suprapun între animalele rezervării.");
        }

        _dbContext.BookingChangeRequests.Add(changeRequest);

        _dbContext.BookingHistories.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            PerformedByUserId = userId,
            Action = BookingHistoryAction.ChangeRequested,
            PreviousStatus = BookingStatus.Confirmed,
            NewStatus = BookingStatus.Confirmed,
            Description = "Clientul a solicitat modificarea programării.",
            CreatedAt = nowUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return response;
    }


    public async Task ApproveBookingChangeRequestAsync(
        Guid userId,
        Guid bookingId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty ||
            bookingId == Guid.Empty ||
            requestId == Guid.Empty)
        {
            throw new ArgumentException(
                "Identificatorii sunt invalizi.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var booking = await _dbContext.Bookings
                .FromSqlInterpolated(
                    $"""
                SELECT *
                FROM "Bookings"
                WHERE "Id" = {bookingId}
                FOR UPDATE
                """)
                .SingleOrDefaultAsync(cancellationToken);

            if (booking is null)
            {
                throw new KeyNotFoundException(
                    "Rezervarea nu a fost găsită.");
            }

            await RequireBookingReviewerAsync(
                userId,
                booking,
                cancellationToken);

            if (booking.Status != BookingStatus.Confirmed)
            {
                throw new InvalidOperationException(
                    "Rezervarea nu mai este confirmată.");
            }

            var changeRequest = await _dbContext.BookingChangeRequests
                .Include(x => x.Appointments)
                .FirstOrDefaultAsync(
                    x => x.Id == requestId &&
                         x.BookingId == bookingId,
                    cancellationToken);

            if (changeRequest is null)
            {
                throw new KeyNotFoundException(
                    "Solicitarea de modificare nu a fost găsită.");
            }

            var nowUtc = DateTime.UtcNow;

            if (changeRequest.Status != BookingChangeRequestStatus.Pending ||
                (changeRequest.ExpiresAt.HasValue &&
                 changeRequest.ExpiresAt.Value <= nowUtc))
            {
                throw new InvalidOperationException(
                    "Solicitarea nu mai este activă sau a expirat.");
            }

            var appointments = await _dbContext.Appointments
                .Where(x => x.BookingId == bookingId)
                .ToListAsync(cancellationToken);

            if (appointments.Count == 0 ||
                appointments.Any(x => x.StartAt <= nowUtc))
            {
                throw new InvalidOperationException(
                    "Rezervarea a început deja sau nu conține programări.");
            }

            var appointmentsById =
                appointments.ToDictionary(x => x.Id);

            var changes = changeRequest.Appointments.ToList();

            if (changes.Count == 0 ||
                changes.Select(x => x.AppointmentId).Distinct().Count()
                    != changes.Count)
            {
                throw new InvalidOperationException(
                    "Solicitarea conține modificări invalide.");
            }

            foreach (var change in changes)
            {
                if (!appointmentsById.TryGetValue(
                    change.AppointmentId,
                    out var appointment))
                {
                    throw new InvalidOperationException(
                        "Solicitarea conține o programare invalidă.");
                }

                if (change.ProposedStartAt.Kind != DateTimeKind.Utc ||
                    change.ProposedEndAt.Kind != DateTimeKind.Utc ||
                    change.ProposedStartAt <= nowUtc ||
                    change.ProposedEndAt != change.ProposedStartAt
                        .AddMinutes(appointment.DurationMinutes))
                {
                    throw new InvalidOperationException(
                        "Intervalul propus nu mai este valid.");
                }
            }

            var changesByAppointmentId =
                changes.ToDictionary(x => x.AppointmentId);

            var finalIntervals = appointments
                .Select(x =>
                {
                    var hasChange = changesByAppointmentId.TryGetValue(
                        x.Id,
                        out var change);

                    return new
                    {
                        AppointmentId = x.Id,
                        Start = hasChange
                            ? change!.ProposedStartAt
                            : x.StartAt,
                        End = hasChange
                            ? change!.ProposedEndAt
                            : x.EndAt
                    };
                })
                .OrderBy(x => x.Start)
                .ToList();

            for (var i = 1; i < finalIntervals.Count; i++)
            {
                if (finalIntervals[i].Start <
                    finalIntervals[i - 1].End)
                {
                    throw new InvalidOperationException(
                        "Programările animalelor se suprapun.");
                }
            }

            var salon = await _dbContext.Businesses
                .AsNoTracking()
                .Where(x => x.Id == booking.BusinessId)
                .Select(x => new
                {
                    x.TimeZoneId,
                    x.IsPublished
                })
                .SingleAsync(cancellationToken);

            if (!salon.IsPublished)
            {
                throw new InvalidOperationException(
                    "Salonul nu mai este publicat.");
            }

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                salon.TimeZoneId);

            var changedAppointmentIds = changes
                .Select(x => x.AppointmentId)
                .ToList();

            // Eliberăm temporar intervalele vechi.
            // Rollback le restaurează dacă aprobarea eșuează.
            await _dbContext.AppointmentSlots
                .Where(x => changedAppointmentIds.Contains(x.AppointmentId))
                .ExecuteDeleteAsync(cancellationToken);

            foreach (var change in changes)
            {
                var appointment =
                    appointmentsById[change.AppointmentId];

                var localDate = DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(
                        change.ProposedStartAt,
                        timeZone));

                var availability =
                    await _availability.GetAvailabilityAsync(
                        booking.BusinessId,
                        booking.BusinessMembershipId,
                        localDate,
                        appointment.DurationMinutes,
                        cancellationToken);

                if (!availability.AvailableStartTimesUtc.Contains(
                    change.ProposedStartAt))
                {
                    throw new InvalidOperationException(
                        "Una dintre orele propuse nu mai este disponibilă.");
                }
            }

            foreach (var change in changes)
            {
                var appointment =
                    appointmentsById[change.AppointmentId];

                appointment.StartAt = change.ProposedStartAt;
                appointment.EndAt = change.ProposedEndAt;
                appointment.UpdatedAt = nowUtc;

                _dbContext.AppointmentSlots.Add(
                    new AppointmentSlot
                    {
                        Id = Guid.NewGuid(),
                        AppointmentId = appointment.Id,
                        BusinessMembershipId =
                            booking.BusinessMembershipId,
                        StartAt = change.ProposedStartAt,
                        EndAt = change.ProposedEndAt,
                        CreatedAt = nowUtc
                    });
            }

            changeRequest.Status =
                BookingChangeRequestStatus.Approved;
            changeRequest.ReviewedByUserId = userId;
            changeRequest.ReviewedAt = nowUtc;
            changeRequest.UpdatedAt = nowUtc;

            booking.UpdatedAt = nowUtc;

            _dbContext.BookingHistories.Add(
                new BookingHistory
                {
                    Id = Guid.NewGuid(),
                    BookingId = bookingId,
                    PerformedByUserId = userId,
                    Action = BookingHistoryAction.ChangeApproved,
                    PreviousStatus = BookingStatus.Confirmed,
                    NewStatus = BookingStatus.Confirmed,
                    Description =
                        "Salonul a aprobat modificarea programării.",
                    CreatedAt = nowUtc
                });

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
            when (
                exception is PostgresException postgres &&
                postgres.SqlState is "23P01" or "40001" or "40P01"
                ||
                exception is DbUpdateException updateException &&
                updateException.InnerException is PostgresException inner &&
                inner.SqlState is "23P01" or "40001" or "40P01"
            )
        {
            await transaction.RollbackAsync(cancellationToken);

            throw new InvalidOperationException(
                "Intervalul propus nu mai este disponibil. Reîncearcă.",
                exception);
        }
    }

    public async Task RejectBookingChangeRequestAsync(
        Guid userId,
        Guid bookingId,
        Guid requestId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty ||
            bookingId == Guid.Empty ||
            requestId == Guid.Empty)
            throw new ArgumentException("Identificatorii sunt invalizi.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(
                "Motivul respingerii este obligatoriu.");

        reason = reason.Trim();

        if (reason.Length > 1000)
            throw new ArgumentException(
                "Motivul nu poate depăși 1000 de caractere.");

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var booking = await _dbContext.Bookings
            .FromSqlInterpolated(
                $"""SELECT * FROM "Bookings" WHERE "Id" = {bookingId} FOR UPDATE""")
            .SingleOrDefaultAsync(cancellationToken);

        if (booking is null)
            throw new KeyNotFoundException("Rezervarea nu a fost găsită.");

        await RequireBookingReviewerAsync(
            userId,
            booking,
            cancellationToken);

        var changeRequest = await _dbContext.BookingChangeRequests
            .FirstOrDefaultAsync(x =>
                x.Id == requestId &&
                x.BookingId == bookingId,
                cancellationToken);

        if (changeRequest is null)
            throw new KeyNotFoundException(
                "Solicitarea de modificare nu a fost găsită.");

        var nowUtc = DateTime.UtcNow;

        if (booking.Status != BookingStatus.Confirmed ||
            changeRequest.Status != BookingChangeRequestStatus.Pending ||
            (changeRequest.ExpiresAt.HasValue &&
             changeRequest.ExpiresAt.Value <= nowUtc))
            throw new InvalidOperationException(
                "Solicitarea nu mai poate fi respinsă.");

        changeRequest.Status = BookingChangeRequestStatus.Rejected;
        changeRequest.ReviewedByUserId = userId;
        changeRequest.ReviewedAt = nowUtc;
        changeRequest.ReviewNote = reason;
        changeRequest.UpdatedAt = nowUtc;

        _dbContext.BookingHistories.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            PerformedByUserId = userId,
            Action = BookingHistoryAction.ChangeRejected,
            PreviousStatus = BookingStatus.Confirmed,
            NewStatus = BookingStatus.Confirmed,
            Description = $"Modificarea a fost respinsă. Motiv: {reason}",
            CreatedAt = nowUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task RequireBookingReviewerAsync(
        Guid userId,
        Booking booking,
        CancellationToken cancellationToken)
    {
        var authorized = await _dbContext.BusinessMemberships
            .AsNoTracking()
            .AnyAsync(x =>
                x.BusinessId == booking.BusinessId &&
                x.UserId == userId &&
                x.IsActive &&
                x.RemovedAt == null &&
                x.User.IsActive &&
                (
                    x.Role == BusinessRole.Owner ||
                    x.Role == BusinessRole.Receptionist ||
                    (
                        x.Role == BusinessRole.Groomer &&
                        x.Id == booking.BusinessMembershipId
                    )
                ),
                cancellationToken);

        if (!authorized)
            throw new UnauthorizedAccessException(
                "Nu ai permisiunea de a gestiona această solicitare.");
    }


    public async Task CancelBookingChangeRequestAsync(
        Guid userId,
        Guid bookingId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty ||
            bookingId == Guid.Empty ||
            requestId == Guid.Empty)
        {
            throw new ArgumentException(
                "Identificatorii sunt invalizi.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var booking = await _dbContext.Bookings
            .FromSqlInterpolated(
                $"""SELECT * FROM "Bookings" WHERE "Id" = {bookingId} FOR UPDATE""")
            .SingleOrDefaultAsync(cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException(
                "Rezervarea nu a fost găsită.");
        }

        if (booking.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "Nu ai permisiunea să anulezi această solicitare.");
        }

        var changeRequest = await _dbContext.BookingChangeRequests
            .FirstOrDefaultAsync(
                x => x.Id == requestId &&
                     x.BookingId == bookingId,
                cancellationToken);

        if (changeRequest is null)
        {
            throw new KeyNotFoundException(
                "Solicitarea de modificare nu a fost găsită.");
        }

        var nowUtc = DateTime.UtcNow;

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Rezervarea nu mai este confirmată.");
        }

        if (changeRequest.Status != BookingChangeRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "Doar solicitările în așteptare pot fi anulate.");
        }

        if (changeRequest.ExpiresAt.HasValue &&
            changeRequest.ExpiresAt.Value <= nowUtc)
        {
            throw new InvalidOperationException(
                "Solicitarea de modificare a expirat.");
        }

        changeRequest.Status = BookingChangeRequestStatus.Cancelled;
        changeRequest.UpdatedAt = nowUtc;

        _dbContext.BookingHistories.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            PerformedByUserId = userId,
            Action = BookingHistoryAction.ChangeCancelled,
            PreviousStatus = BookingStatus.Confirmed,
            NewStatus = BookingStatus.Confirmed,
            Description = "Clientul a anulat solicitarea de modificare.",
            CreatedAt = nowUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

}
