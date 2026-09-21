namespace Pufzi.Data.Database.Entities;

public class Business
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Slug { get; set; }

    public string? Description { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ContactEmail { get; set; }

    public string? AddressLine { get; set; }

    public string? City { get; set; }

    public string? County { get; set; }

    public string? PostalCode { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? LogoBlobName { get; set; }

    public string? CoverImageBlobName { get; set; }

    public string? InstagramUrl { get; set; }

    public string? FacebookUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<BusinessMembership> Memberships { get; set; } = [];

    public ICollection<BusinessInvitation> Invitations { get; set; } = [];

    public ICollection<BusinessImage> Images { get; set; } = [];
}