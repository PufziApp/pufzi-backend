namespace Pufzi.Contracts.Responses.Bookings;

/// <summary>
/// Informațiile unei solicitări de modificare a rezervării.
/// </summary>
public class BookingChangeRequestResponse
{
    /// <summary>
    /// Identificatorul solicitării de modificare.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificatorul rezervării.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// Identificatorul utilizatorului care a solicitat modificarea.
    /// </summary>
    public Guid RequestedByUserId { get; set; }

    /// <summary>
    /// Statusul solicitării.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Motivul modificării.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Data expirării solicitării.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Data creării solicitării.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Programările pentru care au fost propuse modificări.
    /// </summary>
    public List<BookingChangeAppointmentResponse> Appointments
    {
        get;
        set;
    } = [];
}