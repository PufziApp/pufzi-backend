
using System.ComponentModel.DataAnnotations;
using Pufzi.Contracts.Common;

namespace Pufzi.Contracts.Requests.BusinessReviews;

/// <summary>
/// Reprezintă parametrii pentru filtrarea, sortarea și paginarea recenziilor.
/// </summary>
public class BusinessReviewQueryRequest : PaginationRequest
{
    /// <summary>
    /// Filtrează opțional recenziile după numărul de stele, între 1 și 5.
    /// </summary>
    [Range(1, 5)]
    public int? Rating { get; set; }

    /// <summary>
    /// Câmpul după care sunt sortate recenziile.
    /// </summary>
    public BusinessReviewSortBy SortBy { get; set; } =
        BusinessReviewSortBy.CreatedAt;

    /// <summary>
    /// Direcția sortării: crescătoare sau descrescătoare.
    /// </summary>
    public SortDirection SortDirection { get; set; } =
        SortDirection.Desc;

    /// <summary>
    /// Filtrează opțional recenziile în funcție de existența unui răspuns din partea salonului.
    /// </summary>
    public bool? HasBusinessReply { get; set; }
}
