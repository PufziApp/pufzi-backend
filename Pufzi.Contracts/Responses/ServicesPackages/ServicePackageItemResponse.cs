namespace Pufzi.Contracts.Responses.ServicePackages;

/// <summary>
/// Reprezintă un serviciu inclus într-un pachet.
/// </summary>
public class ServicePackageItemResponse
{
    /// <summary>
    /// ID-ul unic al serviciului din pachet.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Numele serviciului.
    /// De exemplu: Tuns, Spălat sau Tăiat gheare.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Configurările serviciului pentru fiecare specie de animal.
    /// </summary>
    public required IReadOnlyCollection<ServicePackageItemOptionResponse>
        Options
    { get; init; }

    /// <summary>
    /// Ordinea serviciului în interiorul pachetului.
    /// </summary>
    public int SortOrder { get; init; }
}