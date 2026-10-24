using Pufzi.Contracts.Responses.AnimalBreeds;

namespace Pufzi.Services.AnimalBreeds;

public interface IAnimalBreedService
{
    Task<IReadOnlyCollection<AnimalBreedResponse>> GetAllAsync(
        Guid animalSpeciesId,
        string? search,
        CancellationToken cancellationToken = default);
}