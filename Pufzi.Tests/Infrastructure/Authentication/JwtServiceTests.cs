using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pufzi.Infrastructure.Authentication;

namespace Pufzi.Tests.Infrastructure.Authentication;

public class JwtServiceTests
{
    private const string SigningKey =
        "THIS_IS_A_TEST_SIGNING_KEY_THAT_IS_LONG_ENOUGH_123456789";

    private static JwtService CreateService(
        int expirationMinutes = 15)
    {
        var options = Options.Create(
            new JwtOptions
            {
                Issuer = "Pufzi.Tests",
                Audience = "Pufzi.Tests",
                SigningKey = SigningKey,
                AccessTokenExpirationMinutes = expirationMinutes
            });

        return new JwtService(options);
    }

    [Fact]
    public void GenerateAccessToken_ShouldCreateValidJwt()
    {
        var service = CreateService();

        var userId = Guid.NewGuid();

        var userData = new JwtUserData
        {
            UserId = userId,
            Email = "test@pufzi.ro",
            EmailConfirmed = true,
            PlatformRole = "User"
        };

        var token = service.GenerateAccessToken(userData);

        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();

        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainExpectedClaims()
    {
        var service = CreateService();

        var userId = Guid.NewGuid();

        var userData = new JwtUserData
        {
            UserId = userId,
            Email = "test@pufzi.ro",
            EmailConfirmed = true,
            PlatformRole = "User"
        };

        var token = service.GenerateAccessToken(userData);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Subject.Should()
            .Be(userId.ToString());

        jwt.Claims
            .Single(x => x.Type == JwtRegisteredClaimNames.Email)
            .Value.Should()
            .Be("test@pufzi.ro");

        jwt.Claims
            .Single(x => x.Type == "email_confirmed")
            .Value.Should()
            .Be("true");

        jwt.Claims
            .Single(x => x.Type == "platform_role")
            .Value.Should()
            .Be("User");

        jwt.Claims.Should()
            .ContainSingle(
                x => x.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateAccessToken_ShouldNotContainBusinessClaims_WhenBusinessIsMissing()
    {
        var service = CreateService();

        var userData = new JwtUserData
        {
            UserId = Guid.NewGuid(),
            Email = "user@pufzi.ro",
            EmailConfirmed = true,
            PlatformRole = "User"
        };

        var token = service.GenerateAccessToken(userData);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Claims.Should()
            .NotContain(x => x.Type == "business_id");

        jwt.Claims.Should()
            .NotContain(x => x.Type == "business_role");
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainBusinessClaims_WhenProvided()
    {
        var service = CreateService();

        var businessId = Guid.NewGuid();

        var userData = new JwtUserData
        {
            UserId = Guid.NewGuid(),
            Email = "owner@pufzi.ro",
            EmailConfirmed = true,
            PlatformRole = "User",
            BusinessId = businessId,
            BusinessRole = "Owner"
        };

        var token = service.GenerateAccessToken(userData);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Claims
            .Single(x => x.Type == "business_id")
            .Value.Should()
            .Be(businessId.ToString());

        jwt.Claims
            .Single(x => x.Type == "business_role")
            .Value.Should()
            .Be("Owner");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateAccessToken_ShouldNotContainBusinessRole_WhenRoleIsEmpty(
        string? role)
    {
        var service = CreateService();

        var userData = new JwtUserData
        {
            UserId = Guid.NewGuid(),
            Email = "user@pufzi.ro",
            EmailConfirmed = false,
            PlatformRole = "User",
            BusinessRole = role
        };

        var token = service.GenerateAccessToken(userData);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Claims.Should()
            .NotContain(x => x.Type == "business_role");
    }

    [Fact]
    public void GenerateAccessToken_ShouldUseConfiguredIssuerAndAudience()
    {
        var service = CreateService();

        var userData = new JwtUserData
        {
            UserId = Guid.NewGuid(),
            Email = "test@pufzi.ro",
            EmailConfirmed = true,
            PlatformRole = "User"
        };

        var token = service.GenerateAccessToken(userData);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Issuer.Should()
            .Be("Pufzi.Tests");

        jwt.Audiences.Should()
            .Contain("Pufzi.Tests");
    }

    [Fact]
    public void GenerateAccessToken_ShouldExpireUsingConfiguredExpiration()
    {
        var before = DateTime.UtcNow;

        var service = CreateService(15);

        var userData = new JwtUserData
        {
            UserId = Guid.NewGuid(),
            Email = "test@pufzi.ro",
            EmailConfirmed = true,
            PlatformRole = "User"
        };

        var token = service.GenerateAccessToken(userData);

        var after = DateTime.UtcNow;

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.ValidTo.Should()
            .BeOnOrAfter(before.AddMinutes(15).AddSeconds(-1));

        jwt.ValidTo.Should()
            .BeOnOrBefore(after.AddMinutes(15).AddSeconds(1));
    }

    [Fact]
    public void GenerateAccessToken_ShouldBeCryptographicallyValid()
    {
        var service = CreateService();

        var userData = new JwtUserData
        {
            UserId = Guid.NewGuid(),
            Email = "test@pufzi.ro",
            EmailConfirmed = true,
            PlatformRole = "User"
        };

        var token = service.GenerateAccessToken(userData);

        var validationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "Pufzi.Tests",

                ValidateAudience = true,
                ValidAudience = "Pufzi.Tests",

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(SigningKey))
            };

        var handler = new JwtSecurityTokenHandler();

        var act = () =>
            handler.ValidateToken(
                token,
                validationParameters,
                out _);

        act.Should().NotThrow();
    }
}