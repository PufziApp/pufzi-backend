using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Services.AnimalSpecies;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/animal-species")]
public class AnimalSpeciesController : ControllerBase
{
    private readonly IAnimalSpeciesService
        _animalSpeciesService;

    public AnimalSpeciesController(
        IAnimalSpeciesService animalSpeciesService)
    {
        _animalSpeciesService =
            animalSpeciesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var response =
            await _animalSpeciesService
                .GetAllAsync(
                    cancellationToken);

        return Ok(response);
    }
}