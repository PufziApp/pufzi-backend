
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.Bookings;
using Pufzi.Services.Bookings;

namespace Pufzi.Api.Controllers.Bookings;

/// <summary>
/// Gestionează confirmarea, respingerea și finalizarea
/// rezervărilor de către salon.
/// </summary>
[ApiController]
[Authorize]
[Route("api/bookings")]
public class BookingReviewController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingReviewController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Confirmă o rezervare aflată în așteptare.
    /// </summary>
    [HttpPut("{bookingId:guid}/confirm")]
    public async Task<IActionResult> ConfirmBooking(
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        await _bookingService.ConfirmBookingAsync(
            userId,
            bookingId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Respinge o rezervare și eliberează intervalele ocupate.
    /// </summary>
    [HttpPut("{bookingId:guid}/reject")]
    public async Task<IActionResult> RejectBooking(
        Guid bookingId,
        [FromBody] RejectBookingRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        await _bookingService.RejectBookingAsync(
            userId,
            bookingId,
            request.Reason,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Marchează o rezervare confirmată ca finalizată.
    /// Toate programările trebuie să se fi încheiat.
    /// </summary>
    [HttpPut("{bookingId:guid}/complete")]
    public async Task<IActionResult> CompleteBooking(
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        await _bookingService.CompleteBookingAsync(
            userId,
            bookingId,
            cancellationToken);

        return NoContent();
    }
}
