namespace Pufzi.Contracts.Requests.Bookings;

/// <summary>
/// Noua dată și oră propusă pentru o programare.
/// </summary>
public class CreateBookingChangeAppointmentRequest
{
    /// <summary>
    /// Identificatorul programării existente.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Noua dată și oră de început, în UTC.
    /// </summary>
    public DateTime ProposedStartAtUtc { get; set; }
}