using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class UserToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public UserTokenType Type { get; set; }

    public required string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;
}