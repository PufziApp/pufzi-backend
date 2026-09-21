namespace Pufzi.Data.Database.Entities;

public class AnimalSpecies
{
    public Guid Id { get; set; }

    public required string Code { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public ICollection<ServiceOption> ServiceOptions { get; set; } = [];
}