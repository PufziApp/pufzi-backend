
using Pufzi.Services.Bookings.Expiration;

namespace Pufzi.Api.BackgroundServices;

public class BookingExpirationBackgroundService : BackgroundService
{
    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingExpirationBackgroundService> _logger;

    public BookingExpirationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingExpirationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Booking expiration background service started.");

        await ProcessExpirationsAsync(stoppingToken);

        using var timer = new PeriodicTimer(CheckInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessExpirationsAsync(stoppingToken);
        }
    }

    private async Task ProcessExpirationsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var expirationService = scope.ServiceProvider
                .GetRequiredService<IBookingExpirationService>();

            var expiredBookings =
                await expirationService.ExpireBookingsAsync(
                    cancellationToken);

            var expiredChangeRequests =
                await expirationService.ExpireChangeRequestsAsync(
                    cancellationToken);

            if (expiredBookings > 0 || expiredChangeRequests > 0)
            {
                _logger.LogInformation(
                    "Booking expiration completed. " +
                    "Expired bookings: {Bookings}. " +
                    "Expired change requests: {ChangeRequests}.",
                    expiredBookings,
                    expiredChangeRequests);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {

        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An error occurred while processing booking expirations.");
        }
    }
}
