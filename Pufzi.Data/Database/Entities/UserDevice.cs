
namespace Pufzi.Data.Database.Entities;

public class UserDevice
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FcmToken { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string Language { get; set; } = "ro";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public User User { get; set; } = null!;
}
