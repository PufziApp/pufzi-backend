using Pufzi.Contracts.Common;

namespace Pufzi.Contracts.Requests.Bookings;

/// <summary>
/// Filtrele și paginarea rezervărilor.
/// </summary>
public class GetBookingsRequest : PaginationRequest
{
    /// <summary>
    /// Statusul rezervărilor căutate.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Identificatorul angajatului.
    /// </summary>
    public Guid? BusinessMembershipId { get; set; }

    /// <summary>
    /// Prima dată din intervalul căutat.
    /// </summary>
    public DateOnly? DateFrom { get; set; }

    /// <summary>
    /// Ultima dată din intervalul căutat.
    /// </summary>
    public DateOnly? DateTo { get; set; }

    /// <summary>
    /// Direcția sortării după data programării.
    /// </summary>
    public SortDirection SortDirection { get; set; } =
        SortDirection.Asc;
}