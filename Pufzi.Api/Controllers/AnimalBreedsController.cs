using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Services.AnimalBreeds;

namespace Pufzi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/animal-breeds")]
public class AnimalBreedsController : ControllerBase
{
    private readonly IAnimalBreedService _service;

    public AnimalBreedsController(IAnimalBreedService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returnează rasele active pentru o specie.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid animalSpeciesId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        if (animalSpeciesId == Guid.Empty)
        {
            return BadRequest("AnimalSpeciesId is required.");
        }

        var result = await _service.GetAllAsync(
            animalSpeciesId,
            search,
            cancellationToken);

        return Ok(result);
    }
}