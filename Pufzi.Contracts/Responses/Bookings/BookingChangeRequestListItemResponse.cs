
namespace Pufzi.Contracts.Responses.Bookings;

public class BookingChangeRequestListItemResponse
{
    /// <summary>
    /// Identificatorul solicitării.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificatorul rezervării.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// Identificatorul salonului.
    /// </summary>
    public Guid SalonId { get; set; }

    /// <summary>
    /// Numele salonului.
    /// </summary>
    public string SalonName { get; set; } = string.Empty;

    /// <summary>
    /// Identificatorul clientului.
    /// </summary>
    public Guid RequestedByUserId { get; set; }

    /// <summary>
    /// Statusul solicitării.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Motivul solicitării.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Numărul programărilor modificate.
    /// </summary>
    public int AppointmentsCount { get; set; }

    /// <summary>
    /// Data expirării solicitării.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Data creării solicitării.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
