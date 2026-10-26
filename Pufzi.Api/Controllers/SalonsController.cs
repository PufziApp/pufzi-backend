using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Salons;
using Pufzi.Contracts.Requests.ServicePackages;
using Pufzi.Contracts.Responses.Salons;
using Pufzi.Contracts.Responses.ServicePackages;
using Pufzi.Services.Salons;

namespace Pufzi.Api.Controllers;

/// <summary>
/// Permite căutarea saloanelor publicate și vizualizarea serviciilor acestora.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/salons")]
public class SalonsController : ControllerBase
{
    private readonly ISalonDiscoveryService _salonDiscoveryService;
    private readonly ISalonPackageService _salonPackageService;

    public SalonsController(
        ISalonDiscoveryService salonDiscoveryService,
        ISalonPackageService salonPackageService)
    {
        _salonDiscoveryService = salonDiscoveryService;
        _salonPackageService = salonPackageService;
    }

    /// <summary>
    /// Returnează lista paginată a saloanelor publicate.
    /// Permite căutare și filtrare după locație.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(SalonListResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<SalonListResponse>> GetSalons(
        [FromQuery] SalonSearchRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _salonDiscoveryService.GetSalonsAsync(
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează profilul public al unui salon.
    /// Sunt disponibile doar saloanele publicate.
    /// </summary>
    [HttpGet("{salonId:guid}")]
    [ProducesResponseType(
        typeof(SalonDetailResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SalonDetailResponse>> GetSalonById(
        Guid salonId,
        CancellationToken cancellationToken)
    {
        var response = await _salonDiscoveryService.GetSalonByIdAsync(
            salonId,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează lista paginată a pachetelor active ale unui salon publicat.
    /// Permite căutare, filtrare și sortare.
    /// </summary>
    [HttpGet("{salonId:guid}/packages")]
    [ProducesResponseType(
        typeof(PagedResponse<ServicePackageResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ServicePackageResponse>>>
        GetSalonPackages(
            Guid salonId,
            [FromQuery] GetServicePackagesRequest request,
            CancellationToken cancellationToken)
    {
        var response = await _salonPackageService.GetAllAsync(
            salonId,
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returnează detaliile unui pachet activ, inclusiv serviciile,
    /// variantele, prețurile, duratele și angajații asociați.
    /// </summary>
    [HttpGet("{salonId:guid}/packages/{packageId:guid}")]
    [ProducesResponseType(
        typeof(ServicePackageDetailsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServicePackageDetailsResponse>>
        GetSalonPackageById(
            Guid salonId,
            Guid packageId,
            CancellationToken cancellationToken)
    {
        var response = await _salonPackageService.GetByIdAsync(
            salonId,
            packageId,
            cancellationToken);

        return Ok(response);
    }
}