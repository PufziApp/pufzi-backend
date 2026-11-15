
namespace Pufzi.Contracts.Notifications.Responses;

/// <summary>
/// Reprezintă o notificare afișată utilizatorului.
/// </summary>
public class NotificationResponse
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Guid? BusinessId { get; set; }

    public Guid? BookingId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
