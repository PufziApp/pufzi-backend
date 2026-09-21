using FluentAssertions;
using Pufzi.Services.EmailTemplates;

namespace Pufzi.Tests.Services.EmailTemplates;

public class AuthEmailTemplatesTests
{
    [Fact]
    public void EmailConfirmation_ShouldContainExpectedContent()
    {
        // Arrange
        const string firstName = "Razvan";
        const string confirmationUrl =
            "https://pufzi.ro/confirm-email?token=test-token";

        // Act
        var result = AuthEmailTemplates.EmailConfirmation(
            firstName,
            confirmationUrl);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();

        result.Should().Contain(
            "Bine ai venit în Pufzi!");

        result.Should().Contain(
            "Salut Razvan,");

        result.Should().Contain(
            "Confirmă adresa de email");

        result.Should().Contain(
            "Linkul este valabil 24 de ore.");

        result.Should().Contain(
            confirmationUrl);
    }

    [Fact]
    public void PasswordReset_ShouldContainExpectedContent()
    {
        // Arrange
        const string firstName = "Razvan";
        const string resetUrl =
            "https://pufzi.ro/reset-password?token=test-token";

        // Act
        var result = AuthEmailTemplates.PasswordReset(
            firstName,
            resetUrl);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();

        result.Should().Contain(
            "Resetare parolă Pufzi");

        result.Should().Contain(
            "Salut Razvan,");

        result.Should().Contain(
            "Resetează parola");

        result.Should().Contain(
            "Linkul este valabil o oră.");

        result.Should().Contain(
            resetUrl);
    }

    [Fact]
    public void EmailConfirmation_ShouldHtmlEncodeFirstName()
    {
        // Arrange
        const string firstName =
            "<script>alert('xss')</script>";

        const string confirmationUrl =
            "https://pufzi.ro/confirm-email";

        // Act
        var result = AuthEmailTemplates.EmailConfirmation(
            firstName,
            confirmationUrl);

        // Assert
        result.Should().NotContain(
            "<script>");

        result.Should().NotContain(
            "</script>");

        result.Should().Contain(
            "&lt;script&gt;");
    }

    [Fact]
    public void PasswordReset_ShouldHtmlEncodeFirstName()
    {
        // Arrange
        const string firstName =
            "<script>alert('xss')</script>";

        const string resetUrl =
            "https://pufzi.ro/reset-password";

        // Act
        var result = AuthEmailTemplates.PasswordReset(
            firstName,
            resetUrl);

        // Assert
        result.Should().NotContain(
            "<script>");

        result.Should().NotContain(
            "</script>");

        result.Should().Contain(
            "&lt;script&gt;");
    }

    [Fact]
    public void EmailConfirmation_ShouldHtmlEncodeConfirmationUrl()
    {
        // Arrange
        const string firstName = "Razvan";

        const string confirmationUrl =
            "https://pufzi.ro/confirm?token=abc&next=\"test\"";

        // Act
        var result = AuthEmailTemplates.EmailConfirmation(
            firstName,
            confirmationUrl);

        // Assert
        result.Should().Contain(
            "token=abc&amp;next=&quot;test&quot;");

        result.Should().NotContain(
            "token=abc&next=\"test\"");
    }

    [Fact]
    public void PasswordReset_ShouldHtmlEncodeResetUrl()
    {
        // Arrange
        const string firstName = "Razvan";

        const string resetUrl =
            "https://pufzi.ro/reset?token=abc&next=\"test\"";

        // Act
        var result = AuthEmailTemplates.PasswordReset(
            firstName,
            resetUrl);

        // Assert
        result.Should().Contain(
            "token=abc&amp;next=&quot;test&quot;");

        result.Should().NotContain(
            "token=abc&next=\"test\"");
    }

    [Fact]
    public void EmailConfirmation_ShouldContainEncodedUrlInsideHref()
    {
        // Arrange
        const string firstName = "Razvan";

        const string confirmationUrl =
            "https://pufzi.ro/confirm?token=abc&value=123";

        // Act
        var result = AuthEmailTemplates.EmailConfirmation(
            firstName,
            confirmationUrl);

        // Assert
        result.Should().Contain(
            "href=\"https://pufzi.ro/confirm?token=abc&amp;value=123\"");
    }

    [Fact]
    public void PasswordReset_ShouldContainEncodedUrlInsideHref()
    {
        // Arrange
        const string firstName = "Razvan";

        const string resetUrl =
            "https://pufzi.ro/reset?token=abc&value=123";

        // Act
        var result = AuthEmailTemplates.PasswordReset(
            firstName,
            resetUrl);

        // Assert
        result.Should().Contain(
            "href=\"https://pufzi.ro/reset?token=abc&amp;value=123\"");
    }
}