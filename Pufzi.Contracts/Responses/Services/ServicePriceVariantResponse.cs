namespace Pufzi.Contracts.Responses.Services;

public class ServicePriceVariantResponse
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public decimal? MinWeightKg { get; init; }

    public decimal? MaxWeightKg { get; init; }

    public decimal? Price { get; init; }

    public bool IsStartingPrice { get; init; }

    public bool IsPriceOnRequest { get; init; }

    public int? DurationMinutes { get; init; }

    public int SortOrder { get; init; }
}