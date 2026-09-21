namespace Pufzi.Infrastructure.Authentication;

public class GoogleUserInfo
{
    public required string Subject { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }
}