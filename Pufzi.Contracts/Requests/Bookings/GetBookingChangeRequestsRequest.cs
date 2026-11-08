
using Pufzi.Contracts.Common;

namespace Pufzi.Contracts.Requests.Bookings;

public class GetBookingChangeRequestsRequest : PaginationRequest
{
    /// <summary>
    /// Filtrează după statusul solicitării.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Filtrează solicitările create începând cu această dată.
    /// </summary>
    public DateOnly? DateFrom { get; set; }

    /// <summary>
    /// Filtrează solicitările create până la această dată inclusiv.
    /// </summary>
    public DateOnly? DateTo { get; set; }

    /// <summary>
    /// Direcția sortării după data creării.
    /// </summary>
    public SortDirection SortDirection { get; set; } =
        SortDirection.Desc;
}
