namespace Pufzi.Services.Bookings.Expiration;

public interface IBookingExpirationService
{
    Task<int> ExpireBookingsAsync(
        CancellationToken cancellationToken = default);

    Task<int> ExpireChangeRequestsAsync(
        CancellationToken cancellationToken = default);
}