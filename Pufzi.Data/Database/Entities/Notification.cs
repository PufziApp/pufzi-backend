
using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }

    public string DataJson { get; set; } = "{}";

    public Guid? BusinessId { get; set; }

    public Guid? BookingId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
