namespace Pufzi.Contracts.Requests.Auth;

/// <summary>
/// Datele necesare pentru autentificarea unui utilizator.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Adresa de email a utilizatorului.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Parola utilizatorului.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Indică dacă sesiunea trebuie păstrată și după închiderea aplicației.
    /// </summary>
    public bool RememberMe { get; init; }
}