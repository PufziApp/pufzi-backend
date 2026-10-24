using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.Businesses;
using Pufzi.Services.Businesses;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/businesses")]
public class BusinessPublicationController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessPublicationController(
        IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpPut("{businessId:guid}/publication")]
    public async Task<IActionResult> UpdatePublication(
        Guid businessId,
        [FromBody] UpdateBusinessPublicationRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        await _businessService.UpdatePublicationAsync(
            userId,
            businessId,
            request.IsPublished,
            cancellationToken);

        return NoContent();
    }
}