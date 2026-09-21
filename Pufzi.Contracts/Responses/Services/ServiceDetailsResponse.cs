namespace Pufzi.Contracts.Responses.Services;

public class ServiceDetailsResponse
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public string? ImageUrl { get; init; }

    public bool IsActive { get; init; }

    public int SortOrder { get; init; }

    public required IReadOnlyCollection<ServiceOptionResponse>
        Options
    { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}