
namespace Pufzi.Data.Database.Entities;

public class NotificationOutbox
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime AvailableAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }

    public Notification Notification { get; set; } = null!;
}
