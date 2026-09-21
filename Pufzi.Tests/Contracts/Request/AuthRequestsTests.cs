using FluentAssertions;
using Pufzi.Contracts.Requests.Auth;

namespace Pufzi.Tests.Contracts.Requests;

public class AuthRequestsTests
{
    [Fact]
    public void ForgotPasswordRequest_ShouldStoreEmail()
    {
        var request =
            new ForgotPasswordRequest
            {
                Email = "test@pufzi.ro"
            };

        request.Email.Should()
            .Be("test@pufzi.ro");
    }

    [Fact]
    public void LoginRequest_ShouldStoreProperties()
    {
        var request =
            new LoginRequest
            {
                Email = "test@pufzi.ro",
                Password = "Password123!"
            };

        request.Email.Should()
            .Be("test@pufzi.ro");

        request.Password.Should()
            .Be("Password123!");
    }

    [Fact]
    public void RefreshTokenRequest_ShouldStoreRefreshToken()
    {
        var request =
            new RefreshTokenRequest
            {
                RefreshToken =
                    "refresh-token-test"
            };

        request.RefreshToken.Should()
            .Be("refresh-token-test");
    }

    [Fact]
    public void RegisterRequest_ShouldStoreProperties()
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

        request.FirstName.Should()
            .Be("Razvan");

        request.LastName.Should()
            .Be("Test");

        request.Email.Should()
            .Be("test@pufzi.ro");

        request.Password.Should()
            .Be("Password123!");

        request.ConfirmPassword.Should()
            .Be("Password123!");
    }

    [Fact]
    public void ResetPasswordRequest_ShouldStoreProperties()
    {
        var request =
            new ResetPasswordRequest
            {
                Token = "reset-token",
                Password = "NewPassword123!",
                ConfirmPassword =
                    "NewPassword123!"
            };

        request.Token.Should()
            .Be("reset-token");

        request.Password.Should()
            .Be("NewPassword123!");

        request.ConfirmPassword.Should()
            .Be("NewPassword123!");
    }
}