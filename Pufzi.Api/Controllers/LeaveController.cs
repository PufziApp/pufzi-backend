using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Leave;
using Pufzi.Contracts.Responses.Leave;
using Pufzi.Services.Leave;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/leave")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    /// <summary>
    /// Returnează soldul de concediu al utilizatorului autentificat
    /// pentru anul specificat sau pentru anul curent.
    /// </summary>
    [HttpGet("balance")]
    public async Task<ActionResult<EmployeeLeaveBalanceResponse>> GetMyBalance(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.GetMyBalanceAsync(
                GetCurrentUserId(),
                businessId,
                year,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează cererile de concediu ale utilizatorului autentificat,
    /// cu suport pentru filtrare, sortare și paginare.
    /// </summary>
    [HttpGet("requests")]
    public async Task<ActionResult<PagedResponse<LeaveRequestResponse>>> GetMyRequests(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromQuery] LeaveRequestsFilterRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.GetMyRequestsAsync(
                GetCurrentUserId(),
                businessId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Creează o nouă cerere de concediu sau absență
    /// pentru utilizatorul autentificat.
    /// </summary>
    [HttpPost("requests")]
    public async Task<ActionResult<LeaveRequestResponse>> CreateRequest(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] CreateLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.CreateRequestAsync(
                GetCurrentUserId(),
                businessId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Anulează o cerere proprie aflată în așteptare.
    /// </summary>
    [HttpPost("requests/{requestId:guid}/cancel")]
    public async Task<ActionResult<LeaveRequestResponse>> CancelRequest(
        Guid requestId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.CancelRequestAsync(
                GetCurrentUserId(),
                businessId,
                requestId,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează cererile de concediu ale echipei.
    /// Operația este disponibilă doar proprietarului salonului.
    /// </summary>
    [HttpGet("team/requests")]
    public async Task<ActionResult<PagedResponse<LeaveRequestResponse>>> GetTeamRequests(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromQuery] LeaveRequestsFilterRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.GetTeamRequestsAsync(
                GetCurrentUserId(),
                businessId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează soldul de concediu al unui membru al echipei
    /// pentru anul specificat sau pentru anul curent.
    /// </summary>
    [HttpGet("team/{userId:guid}/balance")]
    public async Task<ActionResult<EmployeeLeaveBalanceResponse>> GetTeamMemberBalance(
        Guid userId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.GetTeamMemberBalanceAsync(
                GetCurrentUserId(),
                businessId,
                userId,
                year,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Configurează numărul total de zile de concediu anual
    /// pentru un membru al echipei.
    /// Operația este disponibilă doar proprietarului salonului.
    /// </summary>
    [HttpPut("team/{userId:guid}/balance")]
    public async Task<ActionResult<EmployeeLeaveBalanceResponse>> UpdateTeamMemberBalance(
        Guid userId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] UpdateEmployeeLeaveBalanceRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.UpdateTeamMemberBalanceAsync(
                GetCurrentUserId(),
                businessId,
                userId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Aprobă o cerere de concediu aflată în așteptare.
    /// Operația este disponibilă doar proprietarului salonului.
    /// </summary>
    [HttpPost("requests/{requestId:guid}/approve")]
    public async Task<ActionResult<LeaveRequestResponse>> ApproveRequest(
        Guid requestId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] ReviewLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.ApproveRequestAsync(
                GetCurrentUserId(),
                businessId,
                requestId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Respinge o cerere de concediu aflată în așteptare.
    /// Operația este disponibilă doar proprietarului salonului.
    /// </summary>
    [HttpPost("requests/{requestId:guid}/reject")]
    public async Task<ActionResult<LeaveRequestResponse>> RejectRequest(
        Guid requestId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] ReviewLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.RejectRequestAsync(
                GetCurrentUserId(),
                businessId,
                requestId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Revocă o cerere de concediu aprobată.
    /// Pentru concediul anual, zilele utilizate sunt returnate în sold.
    /// Operația este disponibilă doar proprietarului salonului.
    /// </summary>
    [HttpPost("requests/{requestId:guid}/revoke")]
    public async Task<ActionResult<LeaveRequestResponse>> RevokeRequest(
        Guid requestId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] ReviewLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _leaveService.RevokeRequestAsync(
                GetCurrentUserId(),
                businessId,
                requestId,
                request,
                cancellationToken);

        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Utilizatorul autentificat nu a putut fi identificat.");
        }

        return userId;
    }
}