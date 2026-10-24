
namespace Pufzi.Contracts.Responses.Pets;

public class PetResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid AnimalSpeciesId { get; set; }

    public string AnimalSpeciesCode { get; set; } = string.Empty;

    public Guid? AnimalBreedId { get; set; }

    public string? AnimalBreedCode { get; set; }

    public string? Sex { get; set; }

    public DateOnly? BirthDate { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? HeightCm { get; set; }

    public string? Color { get; set; }

    public string MicrochipStatus { get; set; } = string.Empty;

    public string? MicrochipNumber { get; set; }

    public string SterilizationStatus { get; set; } = string.Empty;

    public string ImportantInformationStatus { get; set; } = string.Empty;

    public string? ImportantInformation { get; set; }

    public bool HasProfileImage { get; set; }

    public string? ProfileImageUrl { get; set; }

    public bool IsArchived { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IReadOnlyCollection<PetImageResponse> Images { get; set; }
        = [];
}
