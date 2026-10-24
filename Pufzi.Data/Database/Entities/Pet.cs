using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class Pet
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Name { get; set; }

    public Guid AnimalSpeciesId { get; set; }

    public Guid? AnimalBreedId { get; set; }

    public PetSex? Sex { get; set; }

    public DateOnly? BirthDate { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? HeightCm { get; set; }

    public string? Color { get; set; }

    public PetAnswer MicrochipStatus { get; set; } =
        PetAnswer.Unknown;

    public string? MicrochipNumber { get; set; }

    public PetAnswer SterilizationStatus { get; set; } =
        PetAnswer.Unknown;

    public PetAnswer ImportantInformationStatus { get; set; } =
        PetAnswer.Unknown;

    public string? ImportantInformation { get; set; }

    public string? ProfileImageBlobName { get; set; }

    public bool IsArchived { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public AnimalSpecies AnimalSpecies { get; set; } = null!;

    public AnimalBreed? AnimalBreed { get; set; }

    public ICollection<PetImage> Images { get; set; } = [];
}