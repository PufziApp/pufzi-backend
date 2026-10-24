using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Responses.AnimalBreeds;
using Pufzi.Data.Database;

namespace Pufzi.Services.AnimalBreeds;

public class AnimalBreedService : IAnimalBreedService
{
    private readonly PufziDbContext _dbContext;

    public AnimalBreedService(PufziDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AnimalBreedResponse>> GetAllAsync(
        Guid animalSpeciesId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AnimalBreeds
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.AnimalSpeciesId == animalSpeciesId &&
                x.AnimalSpecies.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Code, $"%{term}%"));
        }

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Code)
            .Select(x => new AnimalBreedResponse
            {
                Id = x.Id,
                AnimalSpeciesId = x.AnimalSpeciesId,
                Code = x.Code
            })
            .ToListAsync(cancellationToken);
    }
}