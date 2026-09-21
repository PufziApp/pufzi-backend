namespace Pufzi.Infrastructure.Authentication;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo> ValidateIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken = default);
}