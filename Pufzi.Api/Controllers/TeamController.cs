using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Team;
using Pufzi.Services.Team;
using System.Security.Claims;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/team")]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamController(
        ITeamService teamService)
    {
        _teamService = teamService;
    }

    /// <summary>
    /// Returnează membrii echipei salonului activ, paginați.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _teamService.GetAllAsync(
            GetCurrentUserId(),
            businessId,
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează profilul detaliat al unui membru al echipei.
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetById(
        Guid userId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        var response = await _teamService.GetByIdAsync(
            GetCurrentUserId(),
            businessId,
            userId,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Modifică datele profesionale și rolul unui membru al echipei.
    /// Operația poate fi efectuată doar de proprietarul salonului.
    /// </summary>
    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update(
        Guid userId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] UpdateTeamMemberRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _teamService.UpdateAsync(
            GetCurrentUserId(),
            businessId,
            userId,
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Activează sau dezactivează un membru al echipei.
    /// Operația poate fi efectuată doar de proprietarul salonului.
    /// </summary>
    [HttpPut("{userId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid userId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] UpdateTeamMemberStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _teamService.UpdateStatusAsync(
            GetCurrentUserId(),
            businessId,
            userId,
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Elimină un membru din echipa salonului.
    /// Membrul este păstrat în baza de date pentru istoric.
    /// Operația poate fi efectuată doar de proprietarul salonului.
    /// </summary>
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Remove(
        Guid userId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        await _teamService.RemoveAsync(
            GetCurrentUserId(),
            businessId,
            userId,
            cancellationToken);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(
                userId,
                out var parsedUserId))
        {
            throw new UnauthorizedAccessException(
                "Utilizatorul autentificat nu este valid.");
        }

        return parsedUserId;
    }
}