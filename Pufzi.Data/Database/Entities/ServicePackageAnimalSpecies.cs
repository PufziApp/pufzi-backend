namespace Pufzi.Data.Database.Entities;

public class ServicePackageAnimalSpecies
{
    public Guid ServicePackageId { get; set; }

    public Guid AnimalSpeciesId { get; set; }

    public ServicePackage ServicePackage { get; set; } = null!;

    public AnimalSpecies AnimalSpecies { get; set; } = null!;
}