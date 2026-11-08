namespace Pufzi.Contracts.Requests.Bookings;

/// <summary>
/// Datele necesare pentru crearea unei rezervări.
/// </summary>
public class CreateBookingRequest
{
    /// <summary>
    /// Identificatorul salonului în care se face rezervarea.
    /// </summary>
    public Guid SalonId { get; set; }

    /// <summary>
    /// Identificatorul angajatului care efectuează serviciile.
    /// </summary>
    public Guid BusinessMembershipId { get; set; }

    /// <summary>
    /// Observațiile transmise salonului de către client.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Programările individuale pentru animalele selectate.
    /// </summary>
    public List<CreateBookingAppointmentRequest> Appointments { get; set; } = [];
}