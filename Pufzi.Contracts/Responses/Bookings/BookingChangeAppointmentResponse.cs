namespace Pufzi.Contracts.Responses.Bookings;

/// <summary>
/// Informațiile privind schimbarea intervalului unei programări.
/// </summary>
public class BookingChangeAppointmentResponse
{
    /// <summary>
    /// Identificatorul programării existente.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Noua dată și oră de început, în UTC.
    /// </summary>
    public DateTime ProposedStartAtUtc { get; set; }

    /// <summary>
    /// Noua dată și oră de sfârșit, în UTC.
    /// </summary>
    public DateTime ProposedEndAtUtc { get; set; }
}