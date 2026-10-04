using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.PublicHolidays;

/// <summary>
/// Filtrele utilizate pentru obținerea sărbătorilor legale.
/// </summary>
public class PublicHolidaysFilterRequest
{
    /// <summary>
    /// Anul pentru care sunt solicitate sărbătorile legale.
    /// </summary>
    [Range(2000, 2100)]
    public int Year { get; set; }

    /// <summary>
    /// Codul ISO al țării, de exemplu RO, HU sau DE.
    /// </summary>
    [Required]
    [StringLength(2, MinimumLength = 2)]
    public required string CountryCode { get; set; }
}