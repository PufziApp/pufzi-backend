namespace Pufzi.Contracts.Responses.ServicePackages;

/// <summary>
/// Reprezintă informațiile complete ale unui pachet de servicii.
/// Folosit pentru vizualizarea și editarea unui pachet individual.
/// </summary>
public class ServicePackageDetailsResponse
{
    /// <summary>
    /// ID-ul unic al pachetului.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Numele pachetului.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Descrierea pachetului.
    /// Poate fi null.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Codul iconiței utilizate pentru afișarea pachetului.
    /// Poate fi null.
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// Indică dacă pachetul este activ.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Ordinea de afișare a pachetului.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Speciile de animale pentru care este disponibil pachetul.
    /// </summary>
    public required IReadOnlyCollection<ServicePackageAnimalSpeciesResponse>
        AnimalSpecies
    { get; init; }

    /// <summary>
    /// Membrii echipei asignați pachetului.
    /// Lista poate fi goală dacă pachetul nu are încă angajați asignați.
    /// </summary>
    public required IReadOnlyCollection<ServicePackageEmployeeResponse>
        Employees
    { get; init; }

    /// <summary>
    /// Serviciile incluse în pachet,
    /// împreună cu speciile, variantele, prețurile și duratele lor.
    /// </summary>
    public required IReadOnlyCollection<ServicePackageItemResponse>
        Items
    { get; init; }

    /// <summary>
    /// Data și ora la care pachetul a fost creat, în UTC.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Data și ora ultimei modificări a pachetului, în UTC.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}