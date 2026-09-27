namespace Pufzi.Contracts.Responses.ServicePackages;

/// <summary>
/// Reprezintă configurarea unui serviciu din pachet
/// pentru o anumită specie de animal.
/// </summary>
public class ServicePackageItemOptionResponse
{
    /// <summary>
    /// ID-ul unic al configurării.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Specia de animal pentru care este valabilă configurarea.
    /// </summary>
    public required ServicePackageAnimalSpeciesResponse AnimalSpecies { get; init; }

    /// <summary>
    /// Variantele de preț și durată disponibile pentru această specie.
    /// </summary>
    public required IReadOnlyCollection<ServicePackageItemVariantResponse>
        Variants
    { get; init; }

    /// <summary>
    /// Ordinea de afișare a configurării.
    /// </summary>
    public int SortOrder { get; init; }
}