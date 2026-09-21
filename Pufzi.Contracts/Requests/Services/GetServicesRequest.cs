using Pufzi.Contracts.Common;

namespace Pufzi.Contracts.Requests.Services;

public class GetServicesRequest : PaginationRequest
{
    public string? Search { get; set; }

    public Guid? AnimalSpeciesId { get; set; }

    public bool? IsActive { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public ServiceSortBy SortBy { get; set; }
        = ServiceSortBy.SortOrder;

    public SortDirection SortDirection { get; set; }
        = SortDirection.Asc;
}