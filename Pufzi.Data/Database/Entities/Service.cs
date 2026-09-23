namespace Pufzi.Data.Database.Entities;

public class Service
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? ImageBlobName { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Business Business { get; set; } = null!;

    public ICollection<ServiceOption> Options { get; set; } = [];
}