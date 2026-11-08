
using System.Data;
using Microsoft.EntityFrameworkCore;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.Bookings;

public partial class BookingService
{
    public async Task CompleteBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || bookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Utilizatorul sau rezervarea este invalidă.");
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
                "Doar rezervările confirmate pot fi finalizate.");
        }

        var nowUtc = DateTime.UtcNow;

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.BookingId == bookingId)
            .Select(x => new
            {
                x.StartAt,
                x.EndAt
            })
            .ToListAsync(cancellationToken);

        if (appointments.Count == 0)
        {
            throw new InvalidOperationException(
                "Rezervarea nu conține programări.");
        }

        if (appointments.Any(x => x.EndAt > nowUtc))
        {
            throw new InvalidOperationException(
                "Nu poți finaliza rezervarea înainte ca toate programările să se încheie.");
        }

        booking.Status = BookingStatus.Completed;
        booking.ExpiresAt = null;
        booking.UpdatedAt = nowUtc;

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
            Action = BookingHistoryAction.Completed,
            PreviousStatus = BookingStatus.Confirmed,
            NewStatus = BookingStatus.Completed,
            Description = "Rezervarea a fost finalizată de salon.",
            CreatedAt = nowUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
