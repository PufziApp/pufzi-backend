using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.Services;
using Pufzi.Services.Services;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServicesController(
        IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromHeader(Name = "X-Business-Id")]
        Guid businessId,
        [FromQuery] GetServicesRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _serviceService.GetAllAsync(
                GetCurrentUserId(),
                businessId,
                request,
                cancellationToken);

        return Ok(response);
    }

    [HttpGet("{serviceId:guid}")]
    public async Task<IActionResult> GetById(
        Guid serviceId,
        [FromHeader(Name = "X-Business-Id")]
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var response =
            await _serviceService.GetByIdAsync(
                GetCurrentUserId(),
                businessId,
                serviceId,
                cancellationToken);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Business-Id")]
        Guid businessId,
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _serviceService.CreateAsync(
                GetCurrentUserId(),
                businessId,
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                serviceId = response.Id
            },
            response);
    }

    [HttpPost("{serviceId:guid}/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(
    Guid serviceId,
    [FromHeader(Name = "X-Business-Id")] Guid businessId,
    IFormFile file,
    CancellationToken cancellationToken)
    {
        await using var stream =
            file.OpenReadStream();

        var extension =
            Path.GetExtension(file.FileName);

        var response =
            await _serviceService.UploadImageAsync(
                GetCurrentUserId(),
                businessId,
                serviceId,
                stream,
                file.ContentType,
                extension,
                cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{serviceId:guid}/image")]
    public async Task<IActionResult> DeleteImage(
        Guid serviceId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        await _serviceService.DeleteImageAsync(
            GetCurrentUserId(),
            businessId,
            serviceId,
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{serviceId:guid}")]
    public async Task<IActionResult> Update(
        Guid serviceId,
        [FromHeader(Name = "X-Business-Id")]
        Guid businessId,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _serviceService.UpdateAsync(
                GetCurrentUserId(),
                businessId,
                serviceId,
                request,
                cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{serviceId:guid}")]
    public async Task<IActionResult> Delete(
        Guid serviceId,
        [FromHeader(Name = "X-Business-Id")]
        Guid businessId,
        CancellationToken cancellationToken)
    {
        await _serviceService.DeleteAsync(
            GetCurrentUserId(),
            businessId,
            serviceId,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{serviceId:guid}/restore")]
    public async Task<IActionResult> Restore(
        Guid serviceId,
        [FromHeader(Name = "X-Business-Id")]
        Guid businessId,
        CancellationToken cancellationToken)
    {
        await _serviceService.RestoreAsync(
            GetCurrentUserId(),
            businessId,
            serviceId,
            cancellationToken);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(
                value,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "Utilizatorul autentificat nu este valid.");
        }

        return userId;
    }
}