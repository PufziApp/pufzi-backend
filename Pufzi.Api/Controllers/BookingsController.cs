
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Bookings;
using Pufzi.Contracts.Responses.Bookings;
using Pufzi.Services.Bookings;

namespace Pufzi.Api.Controllers.Bookings;

/// <summary>
/// Gestionează crearea și consultarea rezervărilor.
/// </summary>
[ApiController]
[Authorize]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Creează o rezervare pentru unul sau mai multe animale.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var response = await _bookingService.CreateBookingAsync(
            userId,
            request,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    /// <summary>
    /// Returnează rezervările utilizatorului autentificat,
    /// cu paginare și filtre.
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<PagedResponse<BookingListItemResponse>>>
        GetMyBookings(
            [FromQuery] GetBookingsRequest request,
            CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var response = await _bookingService.GetMyBookingsAsync(
            userId,
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează rezervările unui salon,
    /// cu paginare și filtre.
    /// </summary>
    [HttpGet("salon/{salonId:guid}")]
    public async Task<ActionResult<PagedResponse<BookingListItemResponse>>>
        GetSalonBookings(
            Guid salonId,
            [FromQuery] GetBookingsRequest request,
            CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var response = await _bookingService.GetSalonBookingsAsync(
            userId,
            salonId,
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează detaliile unei rezervări după identificator.
    /// </summary>
    [HttpGet("{bookingId:guid}")]
    public async Task<ActionResult<BookingResponse>> GetBookingById(
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var response = await _bookingService.GetBookingByIdAsync(
            userId,
            bookingId,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Anulează o rezervare și eliberează intervalele ocupate.
    /// </summary>
    [HttpPut("{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        await _bookingService.CancelBookingAsync(
            userId,
            bookingId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Solicită modificarea uneia sau mai multor programări
    /// dintr-o rezervare.
    /// </summary>
    [HttpPost("{bookingId:guid}/change-requests")]
    public async Task<ActionResult<BookingChangeRequestResponse>>
        CreateChangeRequest(
            Guid bookingId,
            [FromBody] CreateBookingChangeRequest request,
            CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var response =
            await _bookingService.CreateBookingChangeRequestAsync(
                userId,
                bookingId,
                request,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    /// <summary>
    /// Aprobă modificarea unei programări
    /// și actualizează intervalele rezervate.
    /// </summary>
    [HttpPut("{bookingId:guid}/change-requests/{requestId:guid}/approve")]
    public async Task<IActionResult> ApproveChangeRequest(
        Guid bookingId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        await _bookingService.ApproveBookingChangeRequestAsync(
            userId,
            bookingId,
            requestId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Respinge o solicitare de modificare
    /// și păstrează programarea originală.
    /// </summary>
    [HttpPut("{bookingId:guid}/change-requests/{requestId:guid}/reject")]
    public async Task<IActionResult> RejectChangeRequest(
        Guid bookingId,
        Guid requestId,
        [FromBody] RejectBookingRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        await _bookingService.RejectBookingChangeRequestAsync(
            userId,
            bookingId,
            requestId,
            request.Reason,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Anulează o solicitare de modificare
    /// aflată în așteptare.
    /// </summary>
    [HttpPut("{bookingId:guid}/change-requests/{requestId:guid}/cancel")]
    public async Task<IActionResult> CancelChangeRequest(
        Guid bookingId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        await _bookingService.CancelBookingChangeRequestAsync(
            userId,
            bookingId,
            requestId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Returnează solicitările de modificare
    /// ale utilizatorului autentificat.
    /// </summary>
    [HttpGet("change-requests/my")]
    public async Task<ActionResult<PagedResponse<BookingChangeRequestListItemResponse>>>
        GetMyChangeRequests(
            [FromQuery] GetBookingChangeRequestsRequest request,
            CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var response =
            await _bookingService.GetMyBookingChangeRequestsAsync(
                userId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează solicitările de modificare
    /// primite de un salon.
    /// </summary>
    [HttpGet("salon/{salonId:guid}/change-requests")]
    public async Task<ActionResult<PagedResponse<BookingChangeRequestListItemResponse>>>
        GetSalonChangeRequests(
            Guid salonId,
            [FromQuery] GetBookingChangeRequestsRequest request,
            CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var response =
            await _bookingService.GetSalonBookingChangeRequestsAsync(
                userId,
                salonId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează detaliile unei solicitări de modificare.
    /// </summary>
    [HttpGet("{bookingId:guid}/change-requests/{requestId:guid}")]
    public async Task<ActionResult<BookingChangeRequestResponse>>
        GetChangeRequestById(
            Guid bookingId,
            Guid requestId,
            CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var response =
            await _bookingService.GetBookingChangeRequestByIdAsync(
                userId,
                bookingId,
                requestId,
                cancellationToken);

        return Ok(response);
    }


    /// <summary>
    /// Returnează istoricul complet al unei rezervări,
    /// în ordine cronologică.
    /// </summary>
    [HttpGet("{bookingId:guid}/history")]
    public async Task<ActionResult<List<BookingHistoryResponse>>>
        GetBookingHistory(
            Guid bookingId,
            CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var response = await _bookingService.GetBookingHistoryAsync(
            userId,
            bookingId,
            cancellationToken);

        return Ok(response);
    }


    /// <summary>
    /// Anulează o rezervare confirmată din partea salonului.
    /// </summary>
    [HttpPut("{bookingId:guid}/salon-cancel")]
    public async Task<IActionResult> SalonCancelBooking(
        Guid bookingId,
        [FromBody] SalonCancelBookingRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        await _bookingService.SalonCancelBookingAsync(
            userId,
            bookingId,
            request.Reason,
            cancellationToken);

        return NoContent();
    }

}
