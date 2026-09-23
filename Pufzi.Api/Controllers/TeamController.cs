using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Services.Team;

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
    /// Returnează membrii echipei salonului activ.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        var response = await _teamService.GetAllAsync(
            GetCurrentUserId(),
            businessId,
            cancellationToken);

        return Ok(response);
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