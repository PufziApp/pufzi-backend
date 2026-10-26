namespace Pufzi.Contracts.Responses.Pets;

/// <summary>
/// Profilul complet al unui animal înregistrat în Pufzi.
/// Conține informațiile generale, caracteristicile fizice,
/// datele importante și fotografiile animalului.
/// </summary>
public class PetResponse
{
    /// <summary>
    /// Identificatorul unic al animalului.
    /// Va fi utilizat și la crearea programărilor.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificatorul utilizatorului care deține animalul.
    /// Face referire la Id-ul unui utilizator din sistem.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Numele animalului.
    /// </summary>
    /// <example>Max</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identificatorul speciei animalului.
    /// Face referire la Id-ul unei specii din AnimalSpecies.
    /// </summary>
    public Guid AnimalSpeciesId { get; set; }

    /// <summary>
    /// Codul speciei animalului.
    /// Poate fi utilizat de frontend pentru traduceri.
    /// </summary>
    public string AnimalSpeciesCode { get; set; } = string.Empty;

    /// <summary>
    /// Identificatorul rasei animalului, dacă aceasta a fost selectată.
    /// Face referire la Id-ul unei rase din AnimalBreeds.
    /// </summary>
    public Guid? AnimalBreedId { get; set; }

    /// <summary>
    /// Codul rasei animalului, dacă există.
    /// </summary>
    public string? AnimalBreedCode { get; set; }

    /// <summary>
    /// Sexul animalului, dacă a fost completat.
    /// </summary>
    public string? Sex { get; set; }

    /// <summary>
    /// Data nașterii animalului, dacă este cunoscută.
    /// Format: YYYY-MM-DD.
    /// </summary>
    /// <example>2022-05-15</example>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// Greutatea animalului, exprimată în kilograme.
    /// Poate fi utilizată pentru alegerea variantei
    /// corespunzătoare a unui serviciu.
    /// </summary>
    /// <example>7.5</example>
    public decimal? WeightKg { get; set; }

    /// <summary>
    /// Înălțimea animalului, exprimată în centimetri.
    /// </summary>
    /// <example>35</example>
    public decimal? HeightCm { get; set; }

    /// <summary>
    /// Culoarea animalului.
    /// </summary>
    /// <example>Alb</example>
    public string? Color { get; set; }

    /// <summary>
    /// Statusul informațiilor despre microcip.
    /// Valorile acceptate sunt definite de backend.
    /// </summary>
    public string MicrochipStatus { get; set; } = string.Empty;

    /// <summary>
    /// Numărul microcipului, dacă a fost completat.
    /// </summary>
    public string? MicrochipNumber { get; set; }

    /// <summary>
    /// Statusul sterilizării animalului.
    /// Valorile acceptate sunt definite de backend.
    /// </summary>
    public string SterilizationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Statusul informațiilor importante despre animal.
    /// Valorile acceptate sunt definite de backend.
    /// </summary>
    public string ImportantInformationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Informații importante despre animal, dacă există.
    /// Pot include observații relevante pentru îngrijirea acestuia.
    /// </summary>
    public string? ImportantInformation { get; set; }

    /// <summary>
    /// Indică dacă animalul are o fotografie de profil.
    /// </summary>
    public bool HasProfileImage { get; set; }

    /// <summary>
    /// Adresa URL a fotografiei de profil.
    /// Este null dacă nu există o fotografie disponibilă.
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Indică dacă profilul animalului este arhivat.
    /// Animalele arhivate sunt păstrate în sistem pentru istoric.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Data și ora arhivării animalului, dacă este arhivat.
    /// </summary>
    public DateTime? ArchivedAt { get; set; }

    /// <summary>
    /// Data și ora creării profilului animalului.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data și ora ultimei actualizări a profilului.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Fotografiile din galeria animalului.
    /// Fiecare fotografie conține identificatorul,
    /// adresa URL și informațiile despre ordonare.
    /// </summary>
    public IReadOnlyCollection<PetImageResponse> Images { get; set; }
        = [];
}