using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.Team;
using Pufzi.Services.Team;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/team/invitations")]
public class TeamInvitationsController : ControllerBase
{
    private readonly ITeamInvitationService _teamInvitationService;

    public TeamInvitationsController(
        ITeamInvitationService teamInvitationService)
    {
        _teamInvitationService = teamInvitationService;
    }

    /// <summary>
    /// Returnează invitațiile trimise pentru salonul activ.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        var response = await _teamInvitationService.GetAllAsync(
            GetCurrentUserId(),
            businessId,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Trimite o invitație unei persoane pentru a se alătura echipei salonului.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] CreateBusinessInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _teamInvitationService.CreateAsync(
            GetCurrentUserId(),
            businessId,
            request,
            cancellationToken);

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Verifică o invitație înainte ca utilizatorul să se autentifice sau să își creeze cont.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("validate")]
    public async Task<IActionResult> Validate(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var response = await _teamInvitationService.ValidateAsync(
            token,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Acceptă invitația și adaugă utilizatorul autentificat în echipa salonului.
    /// </summary>
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromBody] AcceptBusinessInvitationRequest request,
        CancellationToken cancellationToken)
    {
        await _teamInvitationService.AcceptAsync(
            GetCurrentUserId(),
            request,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Anulează o invitație care nu a fost încă acceptată.
    /// </summary>
    [HttpDelete("{invitationId:guid}")]
    public async Task<IActionResult> Revoke(
        Guid invitationId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        await _teamInvitationService.RevokeAsync(
            GetCurrentUserId(),
            businessId,
            invitationId,
            cancellationToken);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedAccessException(
                "Utilizatorul autentificat nu este valid.");
        }

        return parsedUserId;
    }
}