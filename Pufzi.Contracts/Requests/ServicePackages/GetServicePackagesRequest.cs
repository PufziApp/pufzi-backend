using System.ComponentModel.DataAnnotations;
using Pufzi.Contracts.Common;

namespace Pufzi.Contracts.Requests.ServicePackages;

/// <summary>
/// Reprezintă filtrele, sortarea și paginarea
/// disponibile pentru lista de pachete de servicii.
/// </summary>
public class GetServicePackagesRequest : PaginationRequest
{
    /// <summary>
    /// Text folosit pentru căutarea pachetelor după nume.
    /// </summary>
    [MaxLength(150)]
    public string? Search { get; init; }

    /// <summary>
    /// Filtrează pachetele după o anumită specie de animal.
    /// </summary>
    public Guid? AnimalSpeciesId { get; init; }

    /// <summary>
    /// Filtrează pachetele după un anumit angajat.
    /// Valoarea reprezintă ID-ul BusinessMembership-ului.
    /// </summary>
    public Guid? EmployeeMembershipId { get; init; }

    /// <summary>
    /// Filtrează pachetele după starea lor.
    /// True pentru active, false pentru inactive,
    /// iar null pentru toate.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Prețul minim folosit pentru filtrare.
    /// Poate fi null.
    /// </summary>
    [Range(0, 1_000_000)]
    public decimal? MinPrice { get; init; }

    /// <summary>
    /// Prețul maxim folosit pentru filtrare.
    /// Poate fi null.
    /// </summary>
    [Range(0, 1_000_000)]
    public decimal? MaxPrice { get; init; }

    /// <summary>
    /// Câmpul după care se sortează rezultatele.
    /// </summary>
    public ServicePackageSortBy SortBy { get; init; } =
        ServicePackageSortBy.SortOrder;

    /// <summary>
    /// Direcția sortării.
    /// </summary>
    public SortDirection SortDirection { get; init; } =
        SortDirection.Asc;
}