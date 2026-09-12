namespace Pufzi.Contracts.Requests.Auth;

public class ResetPasswordRequest
{
    public required string Token { get; init; }

    public required string Password { get; init; }

    public required string ConfirmPassword { get; init; }
}