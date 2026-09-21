namespace Pufzi.Data.Database.Entities;

public class ServicePriceVariant
{
    public Guid Id { get; set; }

    public Guid ServiceOptionId { get; set; }

    public required string Name { get; set; }

    public decimal? MinWeightKg { get; set; }

    public decimal? MaxWeightKg { get; set; }

    public decimal? Price { get; set; }

    public bool IsStartingPrice { get; set; }

    public bool IsPriceOnRequest { get; set; }

    public int? DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    public ServiceOption ServiceOption { get; set; } = null!;
}