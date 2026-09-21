using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Responses.Services;
using Pufzi.Data.Database;

namespace Pufzi.Services.AnimalSpecies;

public class AnimalSpeciesService
    : IAnimalSpeciesService
{
    private readonly PufziDbContext _dbContext;

    public AnimalSpeciesService(
        PufziDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AnimalSpeciesResponse>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.AnimalSpecies
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Code)
            .Select(x =>
                new AnimalSpeciesResponse
                {
                    Id = x.Id,
                    Code = x.Code
                })
            .ToListAsync(cancellationToken);
    }
}