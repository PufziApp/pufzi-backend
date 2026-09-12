namespace Pufzi.Contracts.Responses.Businesses;

public class BusinessResponse
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Slug { get; init; }

    public string? Description { get; init; }

    public string? PhoneNumber { get; init; }

    public string? ContactEmail { get; init; }

    public string? AddressLine { get; init; }

    public string? City { get; init; }

    public string? County { get; init; }

    public string? PostalCode { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public string? LogoUrl { get; init; }

    public string? CoverImageUrl { get; init; }

    public string? InstagramUrl { get; init; }

    public string? FacebookUrl { get; init; }

    public required string CurrentUserRole { get; init; }

    public IReadOnlyCollection<BusinessImageResponse> Images { get; init; }
        = [];
}