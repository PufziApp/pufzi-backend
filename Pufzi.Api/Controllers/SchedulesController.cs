using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.Schedules;
using Pufzi.Services.Schedules;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/schedules")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(
        IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    /// <summary>
    /// Returnează programul săptămânal al salonului activ.
    /// Operația este disponibilă tuturor membrilor activi ai salonului.
    /// </summary>
    [HttpGet("business")]
    public async Task<IActionResult> GetBusinessWorkingHours(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        var response =
            await _scheduleService.GetBusinessWorkingHoursAsync(
                GetCurrentUserId(),
                businessId,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Înlocuiește programul săptămânal al salonului activ.
    /// Operația poate fi efectuată doar de proprietarul salonului.
    /// </summary>
    [HttpPut("business")]
    public async Task<IActionResult> UpdateBusinessWorkingHours(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] UpdateBusinessWorkingHoursRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _scheduleService.UpdateBusinessWorkingHoursAsync(
                GetCurrentUserId(),
                businessId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează programul săptămânal al unui membru activ al echipei.
    /// Operația este disponibilă tuturor membrilor activi ai salonului.
    /// </summary>
    [HttpGet("team/{userId:guid}")]
    public async Task<IActionResult> GetEmployeeWorkingHours(
        Guid userId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        var response =
            await _scheduleService.GetEmployeeWorkingHoursAsync(
                GetCurrentUserId(),
                businessId,
                userId,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Înlocuiește programul săptămânal al unui membru activ al echipei.
    /// Fiecare membru își poate modifica propriul program,
    /// iar proprietarul poate modifica programul oricărui membru.
    /// </summary>
    [HttpPut("team/{userId:guid}")]
    public async Task<IActionResult> UpdateEmployeeWorkingHours(
        Guid userId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] UpdateEmployeeWorkingHoursRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _scheduleService.UpdateEmployeeWorkingHoursAsync(
                GetCurrentUserId(),
                businessId,
                userId,
                request,
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