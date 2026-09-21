using Pufzi.Contracts.Responses.Services;

namespace Pufzi.Services.AnimalSpecies;

public interface IAnimalSpeciesService
{
    Task<IReadOnlyCollection<AnimalSpeciesResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
}