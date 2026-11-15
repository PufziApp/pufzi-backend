
namespace Pufzi.Contracts.Responses.BusinessReviews;

/// <summary>
/// Reprezintă ratingul mediu și distribuția recenziilor unui salon în funcție de numărul de stele.
/// </summary>
public class BusinessReviewSummaryResponse
{
    /// <summary>
    /// Identificatorul salonului evaluat.
    /// </summary>
    public Guid BusinessId { get; set; }

    /// <summary>
    /// Ratingul mediu al salonului, între 0 și 5, calculat din toate recenziile existente.
    /// </summary>
    public decimal AverageRating { get; set; }

    /// <summary>
    /// Numărul total de recenzii primite de salon.
    /// </summary>
    public int TotalReviews { get; set; }

    /// <summary>
    /// Numărul recenziilor care au primit 5 stele.
    /// </summary>
    public int FiveStarCount { get; set; }

    /// <summary>
    /// Numărul recenziilor care au primit 4 stele.
    /// </summary>
    public int FourStarCount { get; set; }

    /// <summary>
    /// Numărul recenziilor care au primit 3 stele.
    /// </summary>
    public int ThreeStarCount { get; set; }

    /// <summary>
    /// Numărul recenziilor care au primit 2 stele.
    /// </summary>
    public int TwoStarCount { get; set; }

    /// <summary>
    /// Numărul recenziilor care au primit 1 stea.
    /// </summary>
    public int OneStarCount { get; set; }
}
