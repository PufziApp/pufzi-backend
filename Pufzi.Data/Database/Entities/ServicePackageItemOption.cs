namespace Pufzi.Data.Database.Entities;

public class ServicePackageItemOption
{
    public Guid Id { get; set; }

    public Guid ServicePackageItemId { get; set; }

    public Guid AnimalSpeciesId { get; set; }

    public int SortOrder { get; set; }

    public ServicePackageItem ServicePackageItem { get; set; } = null!;

    public AnimalSpecies AnimalSpecies { get; set; } = null!;

    public ICollection<ServicePackageItemVariant> Variants { get; set; } = [];
}