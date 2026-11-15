
using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.BusinessReviews;

/// <summary>
/// Reprezintă datele necesare pentru modificarea unei recenzii existente.
/// </summary>
public class UpdateBusinessReviewRequest
{
    /// <summary>
    /// Noul rating acordat salonului, între 1 și 5 stele.
    /// </summary>
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>
    /// Noul comentariu opțional al clientului, de maximum 2000 de caractere.
    /// </summary>
    [StringLength(2000)]
    public string? Comment { get; set; }
}
