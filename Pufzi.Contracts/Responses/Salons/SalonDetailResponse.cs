using Pufzi.Contracts.Responses.Businesses;

namespace Pufzi.Contracts.Responses.Salons;

public class SalonDetailResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? PhoneNumber { get; init; }

    public string? ContactEmail { get; init; }

    public string? AddressLine { get; init; }

    public string? City { get; init; }

    public string? County { get; init; }

    public string? PostalCode { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public string TimeZoneId { get; init; } = string.Empty;

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public string? LogoUrl { get; init; }

    public string? CoverImageUrl { get; init; }

    public string? InstagramUrl { get; init; }

    public string? FacebookUrl { get; init; }

    public IReadOnlyCollection<BusinessImageResponse> Images { get; init; }
        = [];
}