using Pufzi.Contracts.Responses.Salons.Availability;

namespace Pufzi.Services.Bookings;

public interface IAppointmentAvailabilityService
{
    Task<SalonAvailabilityResponse> GetAvailabilityAsync(
        Guid salonId,
        Guid businessMembershipId,
        DateOnly date,
        int durationMinutes,
        CancellationToken cancellationToken = default);
}