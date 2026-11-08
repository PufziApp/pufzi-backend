
namespace Pufzi.Contracts.Responses.Bookings;

/// <summary>
/// Informațiile unei rezervări afișate într-o listă.
/// </summary>
public class BookingListItemResponse
{
    /// <summary>
    /// Identificatorul rezervării.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificatorul salonului.
    /// </summary>
    public Guid SalonId { get; set; }

    /// <summary>
    /// Numele salonului.
    /// </summary>
    public required string SalonName { get; set; }

    /// <summary>
    /// Identificatorul clientului.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identificatorul angajatului asignat.
    /// </summary>
    public Guid BusinessMembershipId { get; set; }

    /// <summary>
    /// Statusul rezervării.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Numărul animalelor programate.
    /// </summary>
    public int PetsCount { get; set; }

    /// <summary>
    /// Prima oră de începere a programărilor, în UTC.
    /// </summary>
    public DateTime StartAtUtc { get; set; }

    /// <summary>
    /// Ultima oră de încheiere a programărilor, în UTC.
    /// </summary>
    public DateTime EndAtUtc { get; set; }

    /// <summary>
    /// Prețul total cunoscut al rezervării.
    /// </summary>
    public decimal? TotalPrice { get; set; }

    /// <summary>
    /// Indică dacă există servicii cu preț la cerere.
    /// </summary>
    public bool HasPriceOnRequest { get; set; }

    /// <summary>
    /// Data creării rezervării.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
