
using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.BusinessReviews;

/// <summary>
/// Reprezintă datele necesare pentru adăugarea sau modificarea răspunsului unui salon la o recenzie.
/// </summary>
public class BusinessReviewReplyRequest
{
    /// <summary>
    /// Textul obligatoriu al răspunsului salonului, de maximum 2000 de caractere.
    /// </summary>
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string Reply { get; set; }
}
