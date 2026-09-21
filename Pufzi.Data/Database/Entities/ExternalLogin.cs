using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class ExternalLogin
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ExternalLoginProvider Provider { get; set; }

    public required string ProviderUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}