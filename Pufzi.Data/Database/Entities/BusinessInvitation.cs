using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class BusinessInvitation
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid InvitedByUserId { get; set; }

    public required string Email { get; set; }

    public BusinessRole Role { get; set; }

    public required string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public Business Business { get; set; } = null!;

    public User InvitedByUser { get; set; } = null!;
}