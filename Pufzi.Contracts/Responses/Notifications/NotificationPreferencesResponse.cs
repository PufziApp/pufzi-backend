
namespace Pufzi.Contracts.Notifications.Responses;

/// <summary>
/// Reprezintă preferințele curente de notificare
/// ale utilizatorului.
/// </summary>
public class NotificationPreferencesResponse
{
    public string PreferredLanguage { get; set; } = "ro";

    public bool PushEnabled { get; set; }

    public bool BookingNotificationsEnabled { get; set; }

    public bool ReminderNotificationsEnabled { get; set; }

    public bool ReviewNotificationsEnabled { get; set; }

    public bool TeamNotificationsEnabled { get; set; }

    public bool LeaveNotificationsEnabled { get; set; }
}
