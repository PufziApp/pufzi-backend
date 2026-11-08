namespace Pufzi.Contracts.Responses.Bookings;

/// <summary>
/// Informațiile unui pachet rezervat.
/// </summary>
public class BookingServiceResponse
{
    /// <summary>
    /// Identificatorul pachetului de servicii.
    /// </summary>
    public Guid ServicePackageId { get; set; }

    /// <summary>
    /// Denumirea pachetului la momentul rezervării.
    /// </summary>
    public required string PackageName { get; set; }

    /// <summary>
    /// Durata totală a pachetului, în minute.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Prețul cunoscut al pachetului.
    /// </summary>
    public decimal? TotalPrice { get; set; }

    /// <summary>
    /// Indică dacă pachetul conține prețuri la cerere.
    /// </summary>
    public bool HasPriceOnRequest { get; set; }
}