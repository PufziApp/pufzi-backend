using Pufzi.Contracts.Responses.AnimalSpecies;

namespace Pufzi.Services.AnimalSpecies;

public interface IAnimalSpeciesService
{
    Task<IReadOnlyCollection<AnimalSpeciesResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
}