using Pufzi.Contracts.Common;

namespace Pufzi.Contracts.Requests.Services;

/// <summary>
/// Parametrii disponibili pentru căutarea, filtrarea,
/// sortarea și paginarea serviciilor.
/// </summary>
public class GetServicesRequest : PaginationRequest
{
    /// <summary>
    /// Text folosit pentru căutarea serviciilor după nume sau descriere.
    /// Opțional.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filtrează serviciile după specia de animal.
    /// Opțional.
    /// </summary>
    public Guid? AnimalSpeciesId { get; set; }

    /// <summary>
    /// Filtrează serviciile după starea lor.
    /// true = servicii active,
    /// false = servicii dezactivate,
    /// null = toate serviciile.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Prețul minim folosit pentru filtrarea serviciilor.
    /// Opțional.
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Prețul maxim folosit pentru filtrarea serviciilor.
    /// Opțional.
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Câmpul după care sunt sortate serviciile.
    /// Implicit: SortOrder.
    /// </summary>
    public ServiceSortBy SortBy { get; set; }
        = ServiceSortBy.SortOrder;

    /// <summary>
    /// Direcția sortării.
    /// Implicit: Asc.
    /// </summary>
    public SortDirection SortDirection { get; set; }
        = SortDirection.Asc;
}