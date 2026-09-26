namespace Pufzi.Contracts.Responses.Auth;

/// <summary>
/// Datele returnate după autentificarea cu succes.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Tokenul JWT folosit pentru autentificarea cererilor către API.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Tokenul folosit pentru reînnoirea sesiunii.
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Data și ora UTC la care expiră access token-ul.
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; init; }

    /// <summary>
    /// Data și ora UTC la care expiră refresh token-ul.
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; init; }

    /// <summary>
    /// Indică dacă sesiunea trebuie păstrată persistent pe dispozitiv.
    /// </summary>
    public bool RememberMe { get; init; }
}