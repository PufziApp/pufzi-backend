
using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Notifications.Requests;

/// <summary>
/// Reprezintă preferințele de notificare
/// actualizate de utilizator.
/// </summary>
public class UpdateNotificationPreferencesRequest
{
    /// <summary>
    /// Limba preferată pentru notificările utilizatorului.
    /// </summary>
    [Required]
    [RegularExpression("^(ro|en|hu)$")]
    public required string PreferredLanguage { get; set; }

    /// <summary>
    /// Indică dacă utilizatorul dorește notificări push.
    /// </summary>
    public bool PushEnabled { get; set; }

    /// <summary>
    /// Indică dacă notificările despre programări sunt activate.
    /// </summary>
    public bool BookingNotificationsEnabled { get; set; }

    /// <summary>
    /// Indică dacă reminderele pentru programări sunt activate.
    /// </summary>
    public bool ReminderNotificationsEnabled { get; set; }

    /// <summary>
    /// Indică dacă notificările despre recenzii sunt activate.
    /// </summary>
    public bool ReviewNotificationsEnabled { get; set; }

    /// <summary>
    /// Indică dacă notificările despre echipă sunt activate.
    /// </summary>
    public bool TeamNotificationsEnabled { get; set; }

    /// <summary>
    /// Indică dacă notificările despre concedii sunt activate.
    /// </summary>
    public bool LeaveNotificationsEnabled { get; set; }
}
