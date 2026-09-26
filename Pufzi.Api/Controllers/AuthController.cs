using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pufzi.Contracts.Requests.Auth;
using Pufzi.Services.Auth;

namespace Pufzi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Creează un cont nou de utilizator.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Confirmă adresa de email folosind tokenul primit.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.ConfirmEmailAsync(
            request.Token,
            cancellationToken);

        return Ok(new
        {
            message = "Adresa de email a fost confirmată."
        });
    }

    /// <summary>
    /// Confirmă adresa de email direct din linkul primit prin email.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("confirm-email")]
    public async Task<ContentResult> ConfirmEmailFromLink(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        await _authService.ConfirmEmailAsync(
            token,
            cancellationToken);

        var html = """
            <!DOCTYPE html>
            <html lang="ro">
            <head>
                <meta charset="UTF-8">

                <meta
                    name="viewport"
                    content="width=device-width, initial-scale=1.0"
                >

                <title>Email confirmat | Pufzi</title>
            </head>

            <body style="
                margin: 0;
                min-height: 100vh;
                background-color: #f7f5f2;
                font-family: Arial, Helvetica, sans-serif;
                color: #242424;
                display: flex;
                align-items: center;
                justify-content: center;
                padding: 24px;
                box-sizing: border-box;
            ">
                <div style="
                    width: 100%;
                    max-width: 520px;
                    background-color: #ffffff;
                    border: 1px solid #ece8e3;
                    border-radius: 24px;
                    padding: 48px 36px;
                    box-sizing: border-box;
                    text-align: center;
                ">
                    <div style="
                        font-size: 30px;
                        font-weight: 800;
                        color: #f47b35;
                        letter-spacing: -1px;
                        margin-bottom: 32px;
                    ">
                        Pufzi
                    </div>

                    <div style="
                        width: 64px;
                        height: 64px;
                        margin: 0 auto 24px auto;
                        border-radius: 50%;
                        background-color: #eef8ee;
                        color: #4f8f54;
                        line-height: 64px;
                        font-size: 30px;
                        font-weight: 700;
                    ">
                        ✓
                    </div>

                    <h1 style="
                        margin: 0 0 16px 0;
                        font-size: 26px;
                        line-height: 1.3;
                        color: #242424;
                    ">
                        Email confirmat cu succes!
                    </h1>

                    <p style="
                        margin: 0 auto;
                        max-width: 400px;
                        color: #68635f;
                        font-size: 16px;
                        line-height: 1.6;
                    ">
                        Adresa ta de email a fost confirmată.
                        Acum poți reveni în Pufzi și te poți
                        autentifica în contul tău.
                    </p>

                    <div style="
                        margin-top: 32px;
                        padding: 16px;
                        background-color: #faf8f5;
                        border: 1px solid #eee9e4;
                        border-radius: 14px;
                        color: #77716c;
                        font-size: 13px;
                        line-height: 1.5;
                    ">
                        Poți închide această pagină în siguranță.
                    </div>

                    <div style="
                        height: 1px;
                        background-color: #eeeae6;
                        margin: 32px 0 24px 0;
                    "></div>

                    <p style="
                        margin: 0;
                        color: #a19b95;
                        font-size: 12px;
                        line-height: 1.5;
                    ">
                        Pufzi 🐾
                    </p>
                </div>
            </body>
            </html>
            """;

        return Content(
            html,
            "text/html; charset=utf-8");
    }

    /// <summary>
    /// Autentifică un utilizator folosind email și parolă.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(
            request,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Generează un access token nou folosind un refresh token valid.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshAsync(
            request.RefreshToken,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Închide sesiunea curentă și revocă refresh tokenul.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(
            request.RefreshToken,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Trimite instrucțiuni pentru resetarea parolei.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            message =
                "Dacă există un cont cu această adresă de email, " +
                "vei primi instrucțiuni pentru resetarea parolei."
        });
    }

    /// <summary>
    /// Resetează parola folosind un token valid.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            message = "Parola a fost schimbată cu succes."
        });
    }

    /// <summary>
    /// Autentifică sau înregistrează un utilizator folosind Google.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _authService.GoogleLoginAsync(
                request,
                cancellationToken);

        return Ok(response);
    }
}