namespace Pufzi.Contracts.Responses.Salons;

public class SalonListItemResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? City { get; init; }

    public string? County { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }

    public string? CoverImageUrl { get; init; }
}