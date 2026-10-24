namespace Pufzi.Data.Database.Entities;

public class AnimalBreed
{
    public Guid Id { get; set; }

    public Guid AnimalSpeciesId { get; set; }

    public required string Code { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public AnimalSpecies AnimalSpecies { get; set; } = null!;

    public ICollection<Pet> Pets { get; set; } = [];
}