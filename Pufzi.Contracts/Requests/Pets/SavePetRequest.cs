using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Pets;

/// <summary>
/// Datele pentru crearea sau actualizarea unui animal.
/// </summary>
public class SavePetRequest
{
    /// <summary>
    /// Numele animalului.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// ID-ul speciei din catalog.
    /// </summary>
    public Guid AnimalSpeciesId { get; init; }

    /// <summary>
    /// ID-ul rasei, dacă este cunoscută.
    /// </summary>
    public Guid? AnimalBreedId { get; init; }

    /// <summary>
    /// Male, Female sau Unknown.
    /// </summary>
    public string? Sex { get; init; }

    /// <summary>
    /// Data nașterii.
    /// </summary>
    public DateOnly? BirthDate { get; init; }

    /// <summary>
    /// Greutatea în kilograme.
    /// </summary>
    [Range(typeof(decimal), "0.01", "99999")]
    public decimal? WeightKg { get; init; }

    /// <summary>
    /// Înălțimea în centimetri.
    /// </summary>
    [Range(typeof(decimal), "0.01", "99999")]
    public decimal? HeightCm { get; init; }

    /// <summary>
    /// Culoarea animalului.
    /// </summary>
    [StringLength(100)]
    public string? Color { get; init; }

    /// <summary>
    /// Yes, No sau Unknown.
    /// </summary>
    public string? MicrochipStatus { get; init; }

    /// <summary>
    /// Numărul microcipului, dacă există.
    /// </summary>
    [StringLength(30)]
    public string? MicrochipNumber { get; init; }

    /// <summary>
    /// Yes, No sau Unknown.
    /// </summary>
    public string? SterilizationStatus { get; init; }

    /// <summary>
    /// Yes, No sau Unknown.
    /// </summary>
    public string? ImportantInformationStatus { get; init; }

    /// <summary>
    /// Alergii, probleme de sănătate sau alte informații.
    /// </summary>
    [StringLength(2000)]
    public string? ImportantInformation { get; init; }
}