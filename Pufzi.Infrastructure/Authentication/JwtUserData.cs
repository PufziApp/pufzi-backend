namespace Pufzi.Infrastructure.Authentication;

public class JwtUserData
{
    public Guid UserId { get; init; }

    public required string Email { get; init; }

    public bool EmailConfirmed { get; init; }

    public required string PlatformRole { get; init; }

    public Guid? BusinessId { get; init; }

    public string? BusinessRole { get; init; }
}