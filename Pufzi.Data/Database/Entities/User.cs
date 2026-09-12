using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class User
{
    public Guid Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public required string NormalizedEmail { get; set; }

    public string? PasswordHash { get; set; }

    public PlatformRole PlatformRole { get; set; } = PlatformRole.User;

    public string? PhoneNumber { get; set; }

    public string? ProfileImageBlobName { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public ICollection<UserToken> UserTokens { get; set; } = [];

    public ICollection<ExternalLogin> ExternalLogins { get; set; } = [];

    public ICollection<BusinessMembership> BusinessMemberships { get; set; } = [];

    public ICollection<BusinessInvitation> SentBusinessInvitations { get; set; } = [];
}