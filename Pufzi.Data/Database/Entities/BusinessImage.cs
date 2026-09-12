namespace Pufzi.Data.Database.Entities;

public class BusinessImage
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public required string BlobName { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public Business Business { get; set; } = null!;
}