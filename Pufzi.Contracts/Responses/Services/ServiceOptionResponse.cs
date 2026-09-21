namespace Pufzi.Contracts.Responses.Services;

public class ServiceOptionResponse
{
    public Guid Id { get; init; }

    public required AnimalSpeciesResponse AnimalSpecies { get; init; }

    public int? DurationMinutes { get; init; }

    public int SortOrder { get; init; }

    public required IReadOnlyCollection<ServicePriceVariantResponse>
        PriceVariants
    { get; init; }
}