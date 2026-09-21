namespace Pufzi.Contracts.Requests.Auth;

public class ConfirmEmailRequest
{
    public required string Token { get; init; }
}