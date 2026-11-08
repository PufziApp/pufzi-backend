namespace Pufzi.Contracts.Requests.Bookings;

/// <summary>
/// Datele necesare pentru solicitarea modificării unei rezervări.
/// </summary>
public class CreateBookingChangeRequest
{
    /// <summary>
    /// Motivul pentru care clientul solicită modificarea.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Programările pentru care se solicită schimbarea orei.
    /// </summary>
    public List<CreateBookingChangeAppointmentRequest> Appointments
    {
        get;
        set;
    } = [];
}