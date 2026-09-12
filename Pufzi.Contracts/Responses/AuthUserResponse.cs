namespace Pufzi.Contracts.Responses.Auth;

public class AuthUserResponse
{
    public Guid Id { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }

    public bool EmailConfirmed { get; init; }

    public required string PlatformRole { get; init; }
}