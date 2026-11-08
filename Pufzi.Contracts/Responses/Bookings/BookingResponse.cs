namespace Pufzi.Contracts.Responses.Bookings;

/// <summary>
/// Informațiile unei rezervări create.
/// </summary>
public class BookingResponse
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
    /// Identificatorul clientului.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identificatorul angajatului selectat.
    /// </summary>
    public Guid BusinessMembershipId { get; set; }

    /// <summary>
    /// Statusul rezervării.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Observațiile clientului.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Momentul expirării rezervării neconfirmate.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Data creării rezervării.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Programările animalelor incluse în rezervare.
    /// </summary>
    public List<BookingAppointmentResponse> Appointments { get; set; } = [];
}