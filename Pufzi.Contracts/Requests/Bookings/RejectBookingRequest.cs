namespace Pufzi.Contracts.Requests.Bookings;

/// <summary>
/// Datele necesare pentru respingerea unei rezervări.
/// </summary>
public class RejectBookingRequest
{
    /// <summary>
    /// Motivul pentru care salonul respinge rezervarea.
    /// </summary>
    public required string Reason { get; set; }
}