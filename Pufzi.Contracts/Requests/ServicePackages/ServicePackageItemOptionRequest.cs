using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.ServicePackages;

/// <summary>
/// Reprezintă configurarea unui serviciu din pachet
/// pentru o anumită specie de animal.
/// </summary>
public class ServicePackageItemOptionRequest
{
    /// <summary>
    /// ID-ul speciei de animal pentru care se aplică această configurare.
    /// </summary>
    [Required]
    public Guid AnimalSpeciesId { get; init; }

    /// <summary>
    /// Variantele disponibile pentru această specie.
    /// De exemplu: Talie mică, Talie medie și Talie mare.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required IReadOnlyCollection<ServicePackageItemVariantRequest> Variants { get; init; }

    /// <summary>
    /// Ordinea de afișare a configurării.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int SortOrder { get; init; }
}