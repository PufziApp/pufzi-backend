using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pufzi.Api.Controllers;
using Pufzi.Contracts.Requests.Auth;
using Pufzi.Services.Auth;

namespace Pufzi.Tests.Api.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock =
            new Mock<IAuthService>();

        _controller =
            new AuthController(
                _authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ShouldCallService_AndReturnOk()
    {
        var request =
            new RegisterRequest
            {
                FirstName = "Razvan",
                LastName = "Test",
                Email = "test@pufzi.ro",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.Register(
                request,
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        _authServiceMock.Verify(
            x => x.RegisterAsync(
                request,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldCallService_AndReturnOk()
    {
        const string token =
            "confirmation-token";

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.ConfirmEmail(
                token,
                cancellationToken);

        var okResult =
            result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

        okResult.Value.Should()
            .NotBeNull();

        var message =
            okResult.Value!
                .GetType()
                .GetProperty("message")!
                .GetValue(okResult.Value)
                ?.ToString();

        message.Should()
            .Be(
                "Adresa de email a fost confirmată.");

        _authServiceMock.Verify(
            x => x.ConfirmEmailAsync(
                token,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Login_ShouldCallService_AndReturnOk()
    {
        var request =
            new LoginRequest
            {
                Email = "test@pufzi.ro",
                Password = "Password123!"
            };

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.Login(
                request,
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        _authServiceMock.Verify(
            x => x.LoginAsync(
                request,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Refresh_ShouldCallService_AndReturnOk()
    {
        var request =
            new RefreshTokenRequest
            {
                RefreshToken =
                    "refresh-token"
            };

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.Refresh(
                request,
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        _authServiceMock.Verify(
            x => x.RefreshAsync(
                request.RefreshToken,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Logout_ShouldCallService_AndReturnNoContent()
    {
        var request =
            new RefreshTokenRequest
            {
                RefreshToken =
                    "refresh-token"
            };

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.Logout(
                request,
                cancellationToken);

        result.Should()
            .BeOfType<NoContentResult>();

        _authServiceMock.Verify(
            x => x.LogoutAsync(
                request.RefreshToken,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_ShouldCallService_AndReturnOk()
    {
        var request =
            new ForgotPasswordRequest
            {
                Email = "test@pufzi.ro"
            };

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.ForgotPassword(
                request,
                cancellationToken);

        var okResult =
            result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

        okResult.Value.Should()
            .NotBeNull();

        var message =
            okResult.Value!
                .GetType()
                .GetProperty("message")!
                .GetValue(okResult.Value)
                ?.ToString();

        message.Should()
            .Be(
                "Dacă există un cont cu această adresă de email, vei primi instrucțiuni pentru resetarea parolei.");

        _authServiceMock.Verify(
            x => x.ForgotPasswordAsync(
                request,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldCallService_AndReturnOk()
    {
        var request =
            new ResetPasswordRequest
            {
                Token = "reset-token",
                Password = "NewPassword123!",
                ConfirmPassword =
                    "NewPassword123!"
            };

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.ResetPassword(
                request,
                cancellationToken);

        var okResult =
            result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

        okResult.Value.Should()
            .NotBeNull();

        var message =
            okResult.Value!
                .GetType()
                .GetProperty("message")!
                .GetValue(okResult.Value)
                ?.ToString();

        message.Should()
            .Be(
                "Parola a fost schimbată cu succes.");

        _authServiceMock.Verify(
            x => x.ResetPasswordAsync(
                request,
                cancellationToken),
            Times.Once);
    }
}