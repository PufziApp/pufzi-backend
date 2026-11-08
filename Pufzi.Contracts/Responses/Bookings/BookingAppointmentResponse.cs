namespace Pufzi.Contracts.Responses.Bookings;

/// <summary>
/// Informațiile unei programări individuale.
/// </summary>
public class BookingAppointmentResponse
{
    /// <summary>
    /// Identificatorul programării.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificatorul animalului programat.
    /// </summary>
    public Guid PetId { get; set; }

    /// <summary>
    /// Ora începerii programării, în UTC.
    /// </summary>
    public DateTime StartAtUtc { get; set; }

    /// <summary>
    /// Ora încheierii programării, în UTC.
    /// </summary>
    public DateTime EndAtUtc { get; set; }

    /// <summary>
    /// Durata totală a programării, în minute.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Prețul total cunoscut al serviciilor.
    /// </summary>
    public decimal? TotalPrice { get; set; }

    /// <summary>
    /// Indică dacă există servicii cu preț la cerere.
    /// </summary>
    public bool HasPriceOnRequest { get; set; }

    /// <summary>
    /// Pachetele de servicii incluse în programare.
    /// </summary>
    public List<BookingServiceResponse> Services { get; set; } = [];
}