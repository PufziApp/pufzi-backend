
using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Responses.Bookings;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.Bookings;

public partial class BookingService
{
    public async Task<List<BookingHistoryResponse>> GetBookingHistoryAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || bookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Utilizatorul sau rezervarea este invalidă.");
        }

        var booking = await _dbContext.Bookings
            .AsNoTracking()
            .Where(x => x.Id == bookingId)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.BusinessId,
                x.BusinessMembershipId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException(
                "Rezervarea nu a fost găsită.");
        }

        if (booking.UserId != userId)
        {
            var authorized = await _dbContext.BusinessMemberships
                .AsNoTracking()
                .AnyAsync(
                    x =>
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
            {
                throw new UnauthorizedAccessException(
                    "Nu ai permisiunea să vezi istoricul acestei rezervări.");
            }
        }

        var history = await _dbContext.BookingHistories
            .AsNoTracking()
            .Where(x => x.BookingId == bookingId)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return history
            .Select(x => new BookingHistoryResponse
            {
                Id = x.Id,
                BookingId = x.BookingId,
                PerformedByUserId = x.PerformedByUserId,
                Action = x.Action.ToString(),
                PreviousStatus = x.PreviousStatus?.ToString(),
                NewStatus = x.NewStatus?.ToString(),
                Description = x.Description,
                CreatedAt = x.CreatedAt
            })
            .ToList();
    }
}
