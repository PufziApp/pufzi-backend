namespace Pufzi.Data.Database.Entities;

public class PetImage
{
    public Guid Id { get; set; }

    public Guid PetId { get; set; }

    public required string BlobName { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public Pet Pet { get; set; } = null!;
}