namespace Pufzi.Contracts.Responses.Auth;

public class AuthResponse
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public DateTime AccessTokenExpiresAt { get; init; }

}