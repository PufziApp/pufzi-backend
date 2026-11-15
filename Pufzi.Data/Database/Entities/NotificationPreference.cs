
namespace Pufzi.Data.Database.Entities;

public class NotificationPreference
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string PreferredLanguage { get; set; } = "ro";

    public bool PushEnabled { get; set; } = true;

    public bool BookingNotificationsEnabled { get; set; } = true;

    public bool ReminderNotificationsEnabled { get; set; } = true;

    public bool ReviewNotificationsEnabled { get; set; } = true;

    public bool TeamNotificationsEnabled { get; set; } = true;

    public bool LeaveNotificationsEnabled { get; set; } = true;

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
