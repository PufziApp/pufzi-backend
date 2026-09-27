using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.ServicePackages;
using Pufzi.Services.ServicePackages;

namespace Pufzi.Api.Controllers;

/// <summary>
/// Gestionează pachetele de servicii ale unui salon.
/// </summary>
[ApiController]
[Authorize]
[Route("api/service-packages")]
public class ServicePackagesController : ControllerBase
{
    private readonly IServicePackageService _servicePackageService;

    public ServicePackagesController(
        IServicePackageService servicePackageService)
    {
        _servicePackageService = servicePackageService;
    }

    /// <summary>
    /// Returnează lista paginată de pachete de servicii ale salonului.
    /// Permite filtrare, căutare și sortare.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromQuery] GetServicePackagesRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _servicePackageService.GetAllAsync(
                GetCurrentUserId(),
                businessId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează informațiile complete ale unui pachet de servicii.
    /// </summary>
    [HttpGet("{packageId:guid}")]
    public async Task<IActionResult> GetById(
        Guid packageId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        var response =
            await _servicePackageService.GetByIdAsync(
                GetCurrentUserId(),
                businessId,
                packageId,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Creează un pachet nou de servicii.
    /// Operația poate fi efectuată doar de proprietarul salonului.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] CreateServicePackageRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _servicePackageService.CreateAsync(
                GetCurrentUserId(),
                businessId,
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                packageId = response.Id
            },
            response);
    }

    /// <summary>
    /// Modifică un pachet de servicii existent.
    /// Operația poate fi efectuată doar de proprietarul salonului.
    /// </summary>
    [HttpPut("{packageId:guid}")]
    public async Task<IActionResult> Update(
        Guid packageId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        [FromBody] UpdateServicePackageRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _servicePackageService.UpdateAsync(
                GetCurrentUserId(),
                businessId,
                packageId,
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Dezactivează un pachet de servicii.
    /// Pachetul rămâne în sistem și poate fi reactivat ulterior.
    /// Operația poate fi efectuată doar de proprietarul salonului.
    /// </summary>
    [HttpDelete("{packageId:guid}")]
    public async Task<IActionResult> Delete(
        Guid packageId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        await _servicePackageService.DeleteAsync(
            GetCurrentUserId(),
            businessId,
            packageId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Reactivează un pachet de servicii dezactivat anterior.
    /// Operația poate fi efectuată doar de proprietarul salonului.
    /// </summary>
    [HttpPost("{packageId:guid}/restore")]
    public async Task<IActionResult> Restore(
        Guid packageId,
        [FromHeader(Name = "X-Business-Id")] Guid businessId,
        CancellationToken cancellationToken)
    {
        await _servicePackageService.RestoreAsync(
            GetCurrentUserId(),
            businessId,
            packageId,
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