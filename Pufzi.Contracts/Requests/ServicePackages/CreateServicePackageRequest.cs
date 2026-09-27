using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.ServicePackages;

/// <summary>
/// Reprezintă datele necesare pentru crearea unui pachet de servicii.
/// </summary>
public class CreateServicePackageRequest
{
    /// <summary>
    /// Numele pachetului.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public required string Name { get; init; }

    /// <summary>
    /// Descrierea pachetului.
    /// Poate fi null.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; init; }

    /// <summary>
    /// Codul iconiței utilizate pentru afișarea pachetului.
    /// De exemplu: scissors, bath, star sau paws.
    /// Poate fi null.
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; init; }

    /// <summary>
    /// ID-urile speciilor de animale pentru care este disponibil pachetul.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required IReadOnlyCollection<Guid> AnimalSpeciesIds { get; init; }

    /// <summary>
    /// ID-urile membership-urilor angajaților care pot presta pachetul.
    /// Lista este opțională și poate fi goală.
    /// </summary>
    public IReadOnlyCollection<Guid> EmployeeMembershipIds { get; init; } = [];

    /// <summary>
    /// Serviciile incluse în pachet.
    /// Pachetul trebuie să conțină cel puțin un serviciu.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required IReadOnlyCollection<ServicePackageItemRequest> Items { get; init; }

    /// <summary>
    /// Ordinea de afișare a pachetului.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int SortOrder { get; init; }
}