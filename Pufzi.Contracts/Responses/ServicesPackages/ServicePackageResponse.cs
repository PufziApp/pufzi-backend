namespace Pufzi.Contracts.Responses.ServicePackages;

/// <summary>
/// Reprezintă informațiile sumarizate ale unui pachet de servicii.
/// Folosit pentru afișarea listelor de pachete.
/// </summary>
public class ServicePackageResponse
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
    /// Cel mai mic preț numeric disponibil în pachet.
    /// Poate fi null dacă nu există niciun preț numeric.
    /// </summary>
    public decimal? StartingPrice { get; init; }

    /// <summary>
    /// Indică dacă pachetul conține cel puțin o variantă
    /// cu preț disponibil la cerere.
    /// </summary>
    public bool HasPriceOnRequest { get; init; }

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
    /// Numărul serviciilor incluse în pachet.
    /// </summary>
    public int ItemsCount { get; init; }

    /// <summary>
    /// Numărul membrilor echipei asignați pachetului.
    /// </summary>
    public int EmployeesCount { get; init; }

    /// <summary>
    /// Data și ora la care pachetul a fost creat, în UTC.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Data și ora ultimei modificări a pachetului, în UTC.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}