using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Services.AnimalSpecies;

namespace Pufzi.Api.Controllers;

/// <summary>
/// Gestionează speciile de animale disponibile în aplicație.
/// </summary>
[ApiController]
[Authorize]
[Route("api/animal-species")]
public class AnimalSpeciesController : ControllerBase
{
    private readonly IAnimalSpeciesService _animalSpeciesService;

    public AnimalSpeciesController(
        IAnimalSpeciesService animalSpeciesService)
    {
        _animalSpeciesService = animalSpeciesService;
    }

    /// <summary>
    /// Returnează lista speciilor de animale disponibile în aplicație.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var response =
            await _animalSpeciesService.GetAllAsync(
                cancellationToken);

        return Ok(response);
    }
}