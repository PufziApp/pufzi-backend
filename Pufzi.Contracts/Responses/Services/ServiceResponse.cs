namespace Pufzi.Contracts.Responses.Services;

public class ServiceResponse
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public string? ImageUrl { get; init; }

    public decimal? StartingPrice { get; init; }

    public bool HasPriceOnRequest { get; init; }

    public bool IsActive { get; init; }

    public int SortOrder { get; init; }

    public required IReadOnlyCollection<AnimalSpeciesResponse>
        AnimalSpecies
    { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}