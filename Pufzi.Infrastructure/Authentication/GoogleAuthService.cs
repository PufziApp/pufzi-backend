using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Pufzi.Infrastructure.Authentication;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleAuthOptions _options;

    public GoogleAuthService(
        IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleUserInfo> ValidateIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new UnauthorizedAccessException(
                "Tokenul Google este invalid.");
        }

        try
        {
            var settings =
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [
                        _options.WebClientId,
                        _options.AndroidClientId,
                        _options.IosClientId
                    ]

                };

            var payload =
                await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    settings);

            if (payload is null ||
                string.IsNullOrWhiteSpace(payload.Subject) ||
                string.IsNullOrWhiteSpace(payload.Email))
            {
                throw new UnauthorizedAccessException(
                    "Tokenul Google este invalid.");
            }

            if (!payload.EmailVerified)
            {
                throw new UnauthorizedAccessException(
                    "Adresa de email Google nu este verificată.");
            }

            return new GoogleUserInfo
            {
                Subject = payload.Subject,
                Email = payload.Email,
                FirstName = payload.GivenName ?? string.Empty,
                LastName = payload.FamilyName ?? string.Empty
            };
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedAccessException(
                "Tokenul Google este invalid.");
        }
    }
}