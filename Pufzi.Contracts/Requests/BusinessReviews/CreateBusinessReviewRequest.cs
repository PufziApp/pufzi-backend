
using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.BusinessReviews;

/// <summary>
/// Reprezintă datele necesare pentru crearea unei recenzii aferente unei rezervări finalizate.
/// </summary>
public class CreateBusinessReviewRequest
{
    /// <summary>
    /// Identificatorul rezervării finalizate pentru care se creează recenzia.
    /// </summary>
    [Required]
    public Guid BookingId { get; set; }

    /// <summary>
    /// Ratingul acordat salonului, între 1 și 5 stele.
    /// </summary>
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>
    /// Comentariul opțional al clientului, de maximum 2000 de caractere.
    /// </summary>
    [StringLength(2000)]
    public string? Comment { get; set; }
}
