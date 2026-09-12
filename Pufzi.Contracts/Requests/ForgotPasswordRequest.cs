namespace Pufzi.Contracts.Requests.Auth;

public class ForgotPasswordRequest
{
    public required string Email { get; init; }
}