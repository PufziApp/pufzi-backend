
namespace Pufzi.Contracts.Notifications.Responses;

/// <summary>
/// Reprezintă o pagină de notificări ale utilizatorului.
/// </summary>
public class NotificationListResponse
{
    public IReadOnlyList<NotificationResponse> Items { get; set; }
        = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int UnreadCount { get; set; }
}
