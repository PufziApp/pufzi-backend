using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Responses.Salons.Availability;
using Pufzi.Services.Bookings;

namespace Pufzi.Api.Controllers.Salons;

/// <summary>
/// Provides public appointment availability for published salons.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/salons/{salonId:guid}/availability")]
public class SalonAvailabilityController : ControllerBase
{
    private readonly IAppointmentAvailabilityService _availabilityService;

    public SalonAvailabilityController(
        IAppointmentAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    /// <summary>
    /// Gets available appointment start times for an employee on a specific date.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(SalonAvailabilityResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SalonAvailabilityResponse>> GetAvailability(
        [FromRoute] Guid salonId,
        [FromQuery] Guid businessMembershipId,
        [FromQuery] DateOnly date,
        [FromQuery] int durationMinutes,
        CancellationToken cancellationToken)
    {
        if (salonId == Guid.Empty)
        {
            return BadRequest("Salon ID is required.");
        }

        if (businessMembershipId == Guid.Empty)
        {
            return BadRequest("Business membership ID is required.");
        }

        if (durationMinutes is < 1 or > 720)
        {
            return BadRequest(
                "Appointment duration must be between 1 and 720 minutes.");
        }

        var response = await _availabilityService.GetAvailabilityAsync(
            salonId,
            businessMembershipId,
            date,
            durationMinutes,
            cancellationToken);

        return Ok(response);
    }
}