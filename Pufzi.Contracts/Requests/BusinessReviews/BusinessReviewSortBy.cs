
namespace Pufzi.Contracts.Requests.BusinessReviews;

/// <summary>
/// Definește câmpurile disponibile pentru sortarea recenziilor.
/// </summary>
public enum BusinessReviewSortBy
{
    /// <summary>
    /// Sortează recenziile după data publicării.
    /// </summary>
    CreatedAt = 1,

    /// <summary>
    /// Sortează recenziile după ratingul acordat.
    /// </summary>
    Rating = 2,

    /// <summary>
    /// Sortează recenziile după data ultimei modificări.
    /// </summary>
    UpdatedAt = 3
}
