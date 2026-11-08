namespace Pufzi.Contracts.Requests.Bookings;

/// <summary>
/// Datele programării pentru un animal.
/// </summary>
public class CreateBookingAppointmentRequest
{
    /// <summary>
    /// Identificatorul animalului care beneficiază de servicii.
    /// </summary>
    public Guid PetId { get; set; }

    /// <summary>
    /// Data și ora începerii programării, în UTC.
    /// </summary>
    public DateTime StartAtUtc { get; set; }

    /// <summary>
    /// Pachetele de servicii selectate pentru animal.
    /// </summary>
    public List<CreateBookingServiceRequest> Services { get; set; } = [];
}