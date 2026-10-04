namespace Pufzi.Contracts.Responses.PublicHolidays;

/// <summary>
/// Reprezintă o sărbătoare legală disponibilă în calendar.
/// </summary>
public class PublicHolidayResponse
{
    /// <summary>
    /// Data sărbătorii legale.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Denumirea sărbătorii legale.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Codul ISO al țării pentru care este definită sărbătoarea.
    /// </summary>
    public required string CountryCode { get; set; }
}