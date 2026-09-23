namespace Pufzi.Contracts.Responses.Services;

/// <summary>
/// Reprezintă informațiile sumarizate ale unui serviciu.
/// Folosit în listele de servicii.
/// </summary>
public class ServiceResponse
{
    /// <summary>
    /// ID-ul unic al serviciului.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Numele serviciului.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Descrierea serviciului.
    /// Poate fi null.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// URL-ul public al imaginii serviciului.
    /// Este null dacă serviciul nu are imagine.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Cel mai mic preț numeric disponibil pentru serviciu.
    /// Poate fi null dacă serviciul nu are niciun preț numeric.
    /// </summary>
    public decimal? StartingPrice { get; init; }

    /// <summary>
    /// Indică dacă serviciul conține cel puțin o variantă
    /// pentru care prețul este disponibil la cerere.
    /// </summary>
    public bool HasPriceOnRequest { get; init; }

    /// <summary>
    /// Indică dacă serviciul este activ.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Ordinea de afișare a serviciului.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Speciile de animale pentru care este disponibil serviciul.
    /// </summary>
    public required IReadOnlyCollection<AnimalSpeciesResponse>
        AnimalSpecies
    { get; init; }

    /// <summary>
    /// Data și ora la care serviciul a fost creat, în UTC.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Data și ora ultimei modificări a serviciului, în UTC.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}