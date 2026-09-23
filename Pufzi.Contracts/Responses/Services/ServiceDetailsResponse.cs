namespace Pufzi.Contracts.Responses.Services;

/// <summary>
/// Reprezintă informațiile complete ale unui serviciu.
/// Folosit pentru vizualizarea și editarea unui serviciu individual.
/// </summary>
public class ServiceDetailsResponse
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
    /// Indică dacă serviciul este activ.
    /// Serviciile dezactivate pot fi reactivate prin restore.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Ordinea de afișare a serviciului.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Configurările serviciului pentru fiecare specie de animal,
    /// inclusiv variantele de preț.
    /// </summary>
    public required IReadOnlyCollection<ServiceOptionResponse>
        Options
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