using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.ServicePackages;

/// <summary>
/// Reprezintă un serviciu inclus într-un pachet.
/// </summary>
public class ServicePackageItemRequest
{
    /// <summary>
    /// Numele serviciului inclus în pachet.
    /// De exemplu: Tuns, Spălat sau Tăiat gheare.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public required string Name { get; init; }

    /// <summary>
    /// Configurările serviciului pentru speciile de animale selectate.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required IReadOnlyCollection<ServicePackageItemOptionRequest> Options { get; init; }

    /// <summary>
    /// Ordinea serviciului în interiorul pachetului.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int SortOrder { get; init; }
}