using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.PublicHolidays;
using Pufzi.Infrastructure.PublicHolidays;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/public-holidays")]
public class PublicHolidaysController : ControllerBase
{
    private readonly IPublicHolidayService _publicHolidayService;

    public PublicHolidaysController(
        IPublicHolidayService publicHolidayService)
    {
        _publicHolidayService = publicHolidayService;
    }

    /// <summary>
    /// Returnează sărbătorile legale pentru țara și anul specificate.
    /// Sărbătorile sunt utilizate pentru afișarea în calendar și
    /// nu închid automat salonul.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPublicHolidays(
        [FromQuery] PublicHolidaysFilterRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _publicHolidayService.GetPublicHolidaysAsync(
                request,
                cancellationToken);

        return Ok(response);
    }
}