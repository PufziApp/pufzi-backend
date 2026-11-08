using System.Data;
using Microsoft.EntityFrameworkCore;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.Bookings;

public partial class BookingService
{
    public async Task SalonCancelBookingAsync(
        Guid userId,
        Guid bookingId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || bookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Utilizatorul sau rezervarea este invalidă.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "Motivul anulării este obligatoriu.");
        }

        reason = reason.Trim();

        if (reason.Length > 1000)
        {
            throw new ArgumentException(
                "Motivul anulării nu poate depăși 1000 de caractere.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

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
                "Doar rezervările confirmate pot fi anulate de salon.");
        }

        var nowUtc = DateTime.UtcNow;

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.BookingId == bookingId)
            .Select(x => new
            {
                x.Id,
                x.StartAt
            })
            .ToListAsync(cancellationToken);

        if (appointments.Count == 0)
        {
            throw new InvalidOperationException(
                "Rezervarea nu conține programări.");
        }

        if (appointments.Any(x => x.StartAt <= nowUtc))
        {
            throw new InvalidOperationException(
                "Nu poți anula o rezervare care a început deja.");
        }

        var appointmentIds = appointments
            .Select(x => x.Id)
            .ToList();

        booking.Status = BookingStatus.Cancelled;
        booking.ExpiresAt = null;
        booking.UpdatedAt = nowUtc;

        await _dbContext.AppointmentSlots
            .Where(x => appointmentIds.Contains(x.AppointmentId))
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.BookingChangeRequests
            .Where(x =>
                x.BookingId == bookingId &&
                x.Status == BookingChangeRequestStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.Status,
                        BookingChangeRequestStatus.Cancelled)
                    .SetProperty(
                        x => x.UpdatedAt,
                        nowUtc),
                cancellationToken);

        _dbContext.BookingHistories.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            PerformedByUserId = userId,
            Action = BookingHistoryAction.Cancelled,
            PreviousStatus = BookingStatus.Confirmed,
            NewStatus = BookingStatus.Cancelled,
            Description = $"Rezervarea a fost anulată de salon. Motiv: {reason}",
            CreatedAt = nowUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
