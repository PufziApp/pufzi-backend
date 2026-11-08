
using System.Data;
using Microsoft.EntityFrameworkCore;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.Bookings.Expiration;

public class BookingExpirationService : IBookingExpirationService
{
    private const int BatchSize = 100;

    private readonly PufziDbContext _dbContext;

    public BookingExpirationService(PufziDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> ExpireBookingsAsync(
        CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;
        var expiredCount = 0;

        // Selectăm doar ID-urile, fără să încărcăm toate relațiile.
        var bookingIds = await _dbContext.Bookings
            .AsNoTracking()
            .Where(x =>
                x.Status == BookingStatus.Pending &&
                x.ExpiresAt.HasValue &&
                x.ExpiresAt.Value <= nowUtc)
            .OrderBy(x => x.ExpiresAt)
            .Select(x => x.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var bookingId in bookingIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted,
                    cancellationToken);

            // PostgreSQL blochează rândul pentru a evita
            // procesarea simultană de către mai multe instanțe.
            var booking = await _dbContext.Bookings
                .FromSqlInterpolated(
                    $"""
                    SELECT *
                    FROM "Bookings"
                    WHERE "Id" = {bookingId}
                    FOR UPDATE
                    """)
                .SingleOrDefaultAsync(cancellationToken);

            if (booking is null ||
                booking.Status != BookingStatus.Pending ||
                !booking.ExpiresAt.HasValue ||
                booking.ExpiresAt.Value > DateTime.UtcNow)
            {
                await transaction.RollbackAsync(cancellationToken);
                continue;
            }

            var expiredAt = DateTime.UtcNow;

            booking.Status = BookingStatus.Expired;
            booking.ExpiresAt = null;
            booking.UpdatedAt = expiredAt;

            var appointmentIds = await _dbContext.Appointments
                .AsNoTracking()
                .Where(x => x.BookingId == bookingId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            await _dbContext.AppointmentSlots
                .Where(x => appointmentIds.Contains(x.AppointmentId))
                .ExecuteDeleteAsync(cancellationToken);

            _dbContext.BookingHistories.Add(new BookingHistory
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                PerformedByUserId = null,
                Action = BookingHistoryAction.Expired,
                PreviousStatus = BookingStatus.Pending,
                NewStatus = BookingStatus.Expired,
                Description =
                    "Rezervarea a expirat automat deoarece nu a fost confirmată.",
                CreatedAt = expiredAt
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Evităm păstrarea entităților urmărite între iterații.
            _dbContext.ChangeTracker.Clear();

            expiredCount++;
        }

        return expiredCount;
    }

    public async Task<int> ExpireChangeRequestsAsync(
        CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;
        var expiredCount = 0;

        var requestIds = await _dbContext.BookingChangeRequests
            .AsNoTracking()
            .Where(x =>
                x.Status == BookingChangeRequestStatus.Pending &&
                x.ExpiresAt.HasValue &&
                x.ExpiresAt.Value <= nowUtc)
            .OrderBy(x => x.ExpiresAt)
            .Select(x => new
            {
                x.Id,
                x.BookingId
            })
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var item in requestIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted,
                    cancellationToken);

            // Folosim aceeași blocare pe Booking ca în
            // approve/reject/cancel pentru modificări.
            var booking = await _dbContext.Bookings
                .FromSqlInterpolated(
                    $"""
                    SELECT *
                    FROM "Bookings"
                    WHERE "Id" = {item.BookingId}
                    FOR UPDATE
                    """)
                .SingleOrDefaultAsync(cancellationToken);

            if (booking is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                continue;
            }

            var changeRequest = await _dbContext.BookingChangeRequests
                .FirstOrDefaultAsync(
                    x => x.Id == item.Id &&
                         x.BookingId == item.BookingId,
                    cancellationToken);

            if (changeRequest is null ||
                changeRequest.Status != BookingChangeRequestStatus.Pending ||
                !changeRequest.ExpiresAt.HasValue ||
                changeRequest.ExpiresAt.Value > DateTime.UtcNow)
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                continue;
            }

            var expiredAt = DateTime.UtcNow;

            changeRequest.Status = BookingChangeRequestStatus.Expired;
            changeRequest.UpdatedAt = expiredAt;

            _dbContext.BookingHistories.Add(new BookingHistory
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                PerformedByUserId = null,
                Action = BookingHistoryAction.ChangeExpired,
                PreviousStatus = booking.Status,
                NewStatus = booking.Status,
                Description =
                    "Solicitarea de modificare a expirat automat.",
                CreatedAt = expiredAt
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _dbContext.ChangeTracker.Clear();

            expiredCount++;
        }

        return expiredCount;
    }
}
