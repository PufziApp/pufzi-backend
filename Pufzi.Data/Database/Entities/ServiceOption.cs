namespace Pufzi.Data.Database.Entities;

public class ServiceOption
{
    public Guid Id { get; set; }

    public Guid ServiceId { get; set; }

    public Guid AnimalSpeciesId { get; set; }

    public int? DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    public Service Service { get; set; } = null!;

    public AnimalSpecies AnimalSpecies { get; set; } = null!;

    public ICollection<ServicePriceVariant> PriceVariants { get; set; } = [];
}