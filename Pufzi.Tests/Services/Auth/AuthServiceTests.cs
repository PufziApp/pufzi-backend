using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Pufzi.Contracts.Requests.Auth;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Infrastructure.Authentication;
using Pufzi.Infrastructure.Email;
using Pufzi.Services.Auth;

namespace Pufzi.Tests.Services.Auth;

public class AuthServiceTests
{
    private static PufziDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<PufziDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new PufziDbContext(options);
    }

    private static IConfiguration CreateConfiguration(
        string? frontendUrl = "https://pufzi.ro",
        int accessTokenMinutes = 15,
        int refreshTokenDays = 30)
    {
        var values =
            new Dictionary<string, string?>();

        if (frontendUrl is not null)
        {
            values["Frontend:WebUrl"] =
                frontendUrl;
        }

        values["Jwt:AccessTokenExpirationMinutes"] =
            accessTokenMinutes.ToString();

        values["Jwt:RefreshTokenExpirationDays"] =
            refreshTokenDays.ToString();

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static User CreateUser(
        string email = "test@pufzi.ro",
        bool emailConfirmed = true,
        bool isActive = true,
        string? passwordHash = "HASH")
    {
        var now = DateTime.UtcNow;

        return new User
        {
            Id = Guid.NewGuid(),

            FirstName = "Test",
            LastName = "User",

            Email = email,
            NormalizedEmail =
                email.ToLowerInvariant(),

            PasswordHash = passwordHash,

            PlatformRole = PlatformRole.User,

            EmailConfirmed = emailConfirmed,
            IsActive = isActive,

            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static AuthService CreateService(
        PufziDbContext dbContext,
        Mock<IPasswordHasher>? passwordHasher = null,
        Mock<ISecureTokenGenerator>? tokenGenerator = null,
        Mock<IJwtService>? jwtService = null,
        Mock<IEmailService>? emailService = null,
        IConfiguration? configuration = null)
    {
        passwordHasher ??=
            new Mock<IPasswordHasher>();

        tokenGenerator ??=
            new Mock<ISecureTokenGenerator>();

        jwtService ??=
            new Mock<IJwtService>();

        emailService ??=
            new Mock<IEmailService>();

        configuration ??=
            CreateConfiguration();

        return new AuthService(
            dbContext,
            passwordHasher.Object,
            tokenGenerator.Object,
            jwtService.Object,
            emailService.Object,
            configuration);
    }

    // =========================================================
    // REGISTER
    // =========================================================

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndConfirmationToken()
    {
        await using var dbContext =
            CreateDbContext();

        var passwordHasher =
            new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(x => x.Hash("Password123!"))
            .Returns("HASHED_PASSWORD");

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x => x.GenerateToken())
            .Returns("confirmation-token");

        tokenGenerator
            .Setup(x =>
                x.HashToken(
                    "confirmation-token"))
            .Returns(
                "confirmation-token-hash");

        var emailService =
            new Mock<IEmailService>();

        emailService
            .Setup(x =>
                x.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service =
            CreateService(
                dbContext,
                passwordHasher,
                tokenGenerator,
                emailService: emailService);

        var request =
            new RegisterRequest
            {
                FirstName = "  Ion  ",
                LastName = "  Popescu  ",
                Email = "  Ion@Pufzi.ro  ",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

        var result =
            await service.RegisterAsync(request);

        result.Message.Should()
            .Be(
                "Contul a fost creat. Verifică emailul pentru confirmare.");

        var user =
            await dbContext.Users
                .SingleAsync();

        user.FirstName.Should()
            .Be("Ion");

        user.LastName.Should()
            .Be("Popescu");

        user.Email.Should()
            .Be("Ion@Pufzi.ro");

        user.NormalizedEmail.Should()
            .Be("ion@pufzi.ro");

        user.PasswordHash.Should()
            .Be("HASHED_PASSWORD");

        user.PlatformRole.Should()
            .Be(PlatformRole.User);

        user.EmailConfirmed.Should()
            .BeFalse();

        user.IsActive.Should()
            .BeTrue();

        var storedToken =
            await dbContext.UserTokens
                .SingleAsync();

        storedToken.UserId.Should()
            .Be(user.Id);

        storedToken.Type.Should()
            .Be(
                UserTokenType.EmailConfirmation);

        storedToken.TokenHash.Should()
            .Be(
                "confirmation-token-hash");

        storedToken.UsedAt.Should()
            .BeNull();

        storedToken.ExpiresAt.Should()
            .BeAfter(storedToken.CreatedAt);

        passwordHasher.Verify(
            x => x.Hash("Password123!"),
            Times.Once);

        emailService.Verify(
            x =>
                x.SendAsync(
                    "Ion@Pufzi.ro",
                    "Confirmă contul Pufzi",
                    It.Is<string>(
                        html =>
                            html.Contains(
                                "confirmation-token")),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenPasswordsDoNotMatch()
    {
        await using var dbContext =
            CreateDbContext();

        var service =
            CreateService(dbContext);

        var request =
            new RegisterRequest
            {
                FirstName = "Ion",
                LastName = "Popescu",
                Email = "ion@pufzi.ro",
                Password = "Password1!",
                ConfirmPassword = "Password2!"
            };

        var action =
            async () =>
                await service.RegisterAsync(
                    request);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Parolele nu coincid.");

        dbContext.Users.Should()
            .BeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        await using var dbContext =
            CreateDbContext();

        dbContext.Users.Add(
            CreateUser(
                "test@pufzi.ro"));

        await dbContext.SaveChangesAsync();

        var service =
            CreateService(dbContext);

        var request =
            new RegisterRequest
            {
                FirstName = "Ion",
                LastName = "Popescu",
                Email = " TEST@PUFZI.RO ",
                Password = "Password123!",
                ConfirmPassword =
                    "Password123!"
            };

        var action =
            async () =>
                await service.RegisterAsync(
                    request);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Există deja un cont cu această adresă de email.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenFrontendUrlIsMissing()
    {
        await using var dbContext =
            CreateDbContext();

        var passwordHasher =
            new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(x =>
                x.Hash(It.IsAny<string>()))
            .Returns("HASH");

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x => x.GenerateToken())
            .Returns("TOKEN");

        tokenGenerator
            .Setup(x =>
                x.HashToken("TOKEN"))
            .Returns("TOKEN_HASH");

        var service =
            CreateService(
                dbContext,
                passwordHasher,
                tokenGenerator,
                configuration:
                    CreateConfiguration(
                        frontendUrl: null));

        var request =
            new RegisterRequest
            {
                FirstName = "Ion",
                LastName = "Popescu",
                Email = "new@pufzi.ro",
                Password = "Password123!",
                ConfirmPassword =
                    "Password123!"
            };

        var action =
            async () =>
                await service.RegisterAsync(
                    request);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Frontend URL is not configured.");
    }

    // =========================================================
    // CONFIRM EMAIL
    // =========================================================

    [Fact]
    public async Task ConfirmEmailAsync_ShouldConfirmEmail()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                emailConfirmed: false);

        var token =
            new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Type =
                    UserTokenType.EmailConfirmation,
                TokenHash = "HASH",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt =
                    DateTime.UtcNow.AddHours(1)
            };

        dbContext.Users.Add(user);
        dbContext.UserTokens.Add(token);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x =>
                x.HashToken("valid-token"))
            .Returns("HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        await service.ConfirmEmailAsync(
            "valid-token");

        user.EmailConfirmed.Should()
            .BeTrue();

        token.UsedAt.Should()
            .NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task ConfirmEmailAsync_ShouldThrow_WhenTokenIsEmpty(
        string token)
    {
        await using var dbContext =
            CreateDbContext();

        var service =
            CreateService(dbContext);

        var action =
            async () =>
                await service.ConfirmEmailAsync(
                    token);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Tokenul de confirmare este invalid.");
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldThrow_WhenTokenDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x =>
                x.HashToken("invalid"))
            .Returns("UNKNOWN_HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.ConfirmEmailAsync(
                    "invalid");

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Tokenul de confirmare este invalid.");
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldThrow_WhenTokenWasAlreadyUsed()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                emailConfirmed: false);

        var userToken =
            new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Type =
                    UserTokenType.EmailConfirmation,
                TokenHash = "HASH",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt =
                    DateTime.UtcNow.AddHours(1),
                UsedAt =
                    DateTime.UtcNow.AddMinutes(-5)
            };

        dbContext.Users.Add(user);
        dbContext.UserTokens.Add(userToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x =>
                x.HashToken("TOKEN"))
            .Returns("HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.ConfirmEmailAsync(
                    "TOKEN");

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Acest link de confirmare a fost deja folosit.");
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldThrow_WhenTokenExpired()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                emailConfirmed: false);

        var userToken =
            new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Type =
                    UserTokenType.EmailConfirmation,
                TokenHash = "HASH",
                CreatedAt =
                    DateTime.UtcNow.AddHours(-2),
                ExpiresAt =
                    DateTime.UtcNow.AddHours(-1)
            };

        dbContext.Users.Add(user);
        dbContext.UserTokens.Add(userToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x =>
                x.HashToken("TOKEN"))
            .Returns("HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.ConfirmEmailAsync(
                    "TOKEN");

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Linkul de confirmare a expirat.");
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var passwordHasher =
            new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(x =>
                x.Verify(
                    "Password123!",
                    "HASH"))
            .Returns(true);

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x => x.GenerateToken())
            .Returns("REFRESH_TOKEN");

        tokenGenerator
            .Setup(x =>
                x.HashToken(
                    "REFRESH_TOKEN"))
            .Returns("REFRESH_HASH");

        var jwtService =
            new Mock<IJwtService>();

        jwtService
            .Setup(x =>
                x.GenerateAccessToken(
                    It.IsAny<JwtUserData>()))
            .Returns("ACCESS_TOKEN");

        var service =
            CreateService(
                dbContext,
                passwordHasher,
                tokenGenerator,
                jwtService);

        var result =
            await service.LoginAsync(
                new LoginRequest
                {
                    Email =
                        " TEST@PUFZI.RO ",
                    Password =
                        "Password123!"
                });

        result.AccessToken.Should()
            .Be("ACCESS_TOKEN");

        result.RefreshToken.Should()
            .Be("REFRESH_TOKEN");


        var storedToken =
            await dbContext.RefreshTokens
                .SingleAsync();

        storedToken.TokenHash.Should()
            .Be("REFRESH_HASH");

        storedToken.UserId.Should()
            .Be(user.Id);

        jwtService.Verify(
            x =>
                x.GenerateAccessToken(
                    It.Is<JwtUserData>(
                        data =>
                            data.UserId ==
                                user.Id &&
                            data.Email ==
                                user.Email &&
                            data.EmailConfirmed &&
                            data.PlatformRole ==
                                "User")),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var service =
            CreateService(dbContext);

        var action =
            async () =>
                await service.LoginAsync(
                    new LoginRequest
                    {
                        Email =
                            "missing@pufzi.ro",
                        Password = "password"
                    });

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Email sau parolă incorectă.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordHashIsMissing()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                passwordHash: null);

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var passwordHasher =
            new Mock<IPasswordHasher>();

        var service =
            CreateService(
                dbContext,
                passwordHasher);

        var action =
            async () =>
                await service.LoginAsync(
                    new LoginRequest
                    {
                        Email = user.Email,
                        Password = "password"
                    });

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Email sau parolă incorectă.");

        passwordHasher.Verify(
            x =>
                x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsWrong()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var passwordHasher =
            new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(x =>
                x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
            .Returns(false);

        var service =
            CreateService(
                dbContext,
                passwordHasher);

        var action =
            async () =>
                await service.LoginAsync(
                    new LoginRequest
                    {
                        Email = user.Email,
                        Password = "wrong"
                    });

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Email sau parolă incorectă.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenAccountIsInactive()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                isActive: false);

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var passwordHasher =
            new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(x =>
                x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
            .Returns(true);

        var service =
            CreateService(
                dbContext,
                passwordHasher);

        var action =
            async () =>
                await service.LoginAsync(
                    new LoginRequest
                    {
                        Email = user.Email,
                        Password = "password"
                    });

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Contul este dezactivat.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenEmailIsNotConfirmed()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                emailConfirmed: false);

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var passwordHasher =
            new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(x =>
                x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
            .Returns(true);

        var service =
            CreateService(
                dbContext,
                passwordHasher);

        var action =
            async () =>
                await service.LoginAsync(
                    new LoginRequest
                    {
                        Email = user.Email,
                        Password = "password"
                    });

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Adresa de email nu a fost confirmată.");
    }

    // =========================================================
    // REFRESH
    // =========================================================

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task RefreshAsync_ShouldThrow_WhenRefreshTokenIsEmpty(
        string refreshToken)
    {
        await using var dbContext =
            CreateDbContext();

        var service =
            CreateService(dbContext);

        var action =
            async () =>
                await service.RefreshAsync(
                    refreshToken);

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Refresh token invalid.");
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenTokenDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x =>
                x.HashToken("UNKNOWN"))
            .Returns("UNKNOWN_HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.RefreshAsync(
                    "UNKNOWN");

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Refresh token invalid sau expirat.");
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenTokenIsRevoked()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        var storedToken =
            CreateRefreshToken(
                user,
                "HASH");

        storedToken.RevokedAt =
            DateTime.UtcNow.AddMinutes(-1);

        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(
            storedToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "TOKEN",
                "HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.RefreshAsync(
                    "TOKEN");

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Refresh token invalid sau expirat.");
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenTokenIsExpired()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        var storedToken =
            CreateRefreshToken(
                user,
                "HASH");

        storedToken.ExpiresAt =
            DateTime.UtcNow.AddMinutes(-1);

        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(
            storedToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "TOKEN",
                "HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.RefreshAsync(
                    "TOKEN");

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Refresh token invalid sau expirat.");
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenUserIsInactive()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                isActive: false);

        var storedToken =
            CreateRefreshToken(
                user,
                "HASH");

        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(
            storedToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "TOKEN",
                "HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.RefreshAsync(
                    "TOKEN");

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Contul este dezactivat.");
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenEmailIsNotConfirmed()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                emailConfirmed: false);

        var storedToken =
            CreateRefreshToken(
                user,
                "HASH");

        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(
            storedToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "TOKEN",
                "HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.RefreshAsync(
                    "TOKEN");

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Adresa de email nu este confirmată.");
    }

    [Fact]
    public async Task RefreshAsync_ShouldRotateRefreshToken()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        var oldToken =
            CreateRefreshToken(
                user,
                "OLD_HASH");

        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(
            oldToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x =>
                x.HashToken("OLD_TOKEN"))
            .Returns("OLD_HASH");

        tokenGenerator
            .Setup(x => x.GenerateToken())
            .Returns("NEW_TOKEN");

        tokenGenerator
            .Setup(x =>
                x.HashToken("NEW_TOKEN"))
            .Returns("NEW_HASH");

        var jwtService =
            new Mock<IJwtService>();

        jwtService
            .Setup(x =>
                x.GenerateAccessToken(
                    It.IsAny<JwtUserData>()))
            .Returns("NEW_ACCESS_TOKEN");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator,
                jwtService:
                    jwtService);

        var result =
            await service.RefreshAsync(
                "OLD_TOKEN");

        result.AccessToken.Should()
            .Be("NEW_ACCESS_TOKEN");

        result.RefreshToken.Should()
            .Be("NEW_TOKEN");

        oldToken.RevokedAt.Should()
            .NotBeNull();

        oldToken.ReplacedByTokenHash.Should()
            .Be("NEW_HASH");

        var tokens =
            await dbContext.RefreshTokens
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

        tokens.Should()
            .HaveCount(2);

        tokens.Should()
            .Contain(
                x =>
                    x.TokenHash ==
                    "NEW_HASH");
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task LogoutAsync_ShouldDoNothing_WhenTokenIsEmpty(
        string token)
    {
        await using var dbContext =
            CreateDbContext();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        await service.LogoutAsync(token);

        tokenGenerator.Verify(
            x =>
                x.HashToken(
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ShouldDoNothing_WhenTokenDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "TOKEN",
                "HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        await service.LogoutAsync("TOKEN");

        dbContext.RefreshTokens.Should()
            .BeEmpty();
    }

    [Fact]
    public async Task LogoutAsync_ShouldDoNothing_WhenTokenAlreadyRevoked()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        var refreshToken =
            CreateRefreshToken(
                user,
                "HASH");

        var revokedAt =
            DateTime.UtcNow.AddMinutes(-10);

        refreshToken.RevokedAt =
            revokedAt;

        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(
            refreshToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "TOKEN",
                "HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        await service.LogoutAsync("TOKEN");

        refreshToken.RevokedAt.Should()
            .Be(revokedAt);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeActiveRefreshToken()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        var refreshToken =
            CreateRefreshToken(
                user,
                "HASH");

        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(
            refreshToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "TOKEN",
                "HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        await service.LogoutAsync("TOKEN");

        refreshToken.RevokedAt.Should()
            .NotBeNull();
    }

    // =========================================================
    // FORGOT PASSWORD
    // =========================================================

    [Fact]
    public async Task ForgotPasswordAsync_ShouldDoNothing_WhenUserDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        var emailService =
            new Mock<IEmailService>();

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator,
                emailService:
                    emailService);

        await service.ForgotPasswordAsync(
            new ForgotPasswordRequest
            {
                Email =
                    "missing@pufzi.ro"
            });

        dbContext.UserTokens.Should()
            .BeEmpty();

        tokenGenerator.Verify(
            x => x.GenerateToken(),
            Times.Never);

        emailService.Verify(
            x =>
                x.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldDoNothing_WhenUserIsInactive()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser(
                isActive: false);

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var emailService =
            new Mock<IEmailService>();

        var service =
            CreateService(
                dbContext,
                emailService:
                    emailService);

        await service.ForgotPasswordAsync(
            new ForgotPasswordRequest
            {
                Email = user.Email
            });

        dbContext.UserTokens.Should()
            .BeEmpty();

        emailService.Verify(
            x =>
                x.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldCreateResetTokenAndSendEmail()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x => x.GenerateToken())
            .Returns("RESET_TOKEN");

        tokenGenerator
            .Setup(x =>
                x.HashToken("RESET_TOKEN"))
            .Returns("RESET_HASH");

        var emailService =
            new Mock<IEmailService>();

        emailService
            .Setup(x =>
                x.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator,
                emailService:
                    emailService);

        await service.ForgotPasswordAsync(
            new ForgotPasswordRequest
            {
                Email =
                    " TEST@PUFZI.RO "
            });

        var storedToken =
            await dbContext.UserTokens
                .SingleAsync();

        storedToken.Type.Should()
            .Be(
                UserTokenType.PasswordReset);

        storedToken.TokenHash.Should()
            .Be("RESET_HASH");

        storedToken.UserId.Should()
            .Be(user.Id);

        emailService.Verify(
            x =>
                x.SendAsync(
                    user.Email,
                    "Resetare parolă Pufzi",
                    It.Is<string>(
                        html =>
                            html.Contains(
                                "RESET_TOKEN")),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // RESET PASSWORD
    // =========================================================

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrow_WhenPasswordsDoNotMatch()
    {
        await using var dbContext =
            CreateDbContext();

        var service =
            CreateService(dbContext);

        var action =
            async () =>
                await service.ResetPasswordAsync(
                    new ResetPasswordRequest
                    {
                        Token = "TOKEN",
                        Password = "One",
                        ConfirmPassword = "Two"
                    });

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Parolele nu coincid.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ResetPasswordAsync_ShouldThrow_WhenTokenIsEmpty(
        string token)
    {
        await using var dbContext =
            CreateDbContext();

        var service =
            CreateService(dbContext);

        var action =
            async () =>
                await service.ResetPasswordAsync(
                    new ResetPasswordRequest
                    {
                        Token = token,
                        Password = "Password123!",
                        ConfirmPassword =
                            "Password123!"
                    });

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Tokenul este invalid.");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrow_WhenTokenDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x =>
                x.HashToken("TOKEN"))
            .Returns("HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.ResetPasswordAsync(
                    new ResetPasswordRequest
                    {
                        Token = "TOKEN",
                        Password = "Password123!",
                        ConfirmPassword =
                            "Password123!"
                    });

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Linkul de resetare este invalid sau a expirat.");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrow_WhenTokenWasAlreadyUsed()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        var userToken =
            CreatePasswordResetToken(
                user,
                "HASH");

        userToken.UsedAt =
            DateTime.UtcNow.AddMinutes(-5);

        dbContext.Users.Add(user);
        dbContext.UserTokens.Add(
            userToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "TOKEN",
                "HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.ResetPasswordAsync(
                    new ResetPasswordRequest
                    {
                        Token = "TOKEN",
                        Password = "NewPassword!",
                        ConfirmPassword =
                            "NewPassword!"
                    });

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Linkul de resetare este invalid sau a expirat.");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrow_WhenTokenExpired()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        var userToken =
            CreatePasswordResetToken(
                user,
                "HASH");

        userToken.ExpiresAt =
            DateTime.UtcNow.AddMinutes(-1);

        dbContext.Users.Add(user);
        dbContext.UserTokens.Add(
            userToken);

        await dbContext.SaveChangesAsync();

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "TOKEN",
                "HASH");

        var service =
            CreateService(
                dbContext,
                tokenGenerator:
                    tokenGenerator);

        var action =
            async () =>
                await service.ResetPasswordAsync(
                    new ResetPasswordRequest
                    {
                        Token = "TOKEN",
                        Password = "NewPassword!",
                        ConfirmPassword =
                            "NewPassword!"
                    });

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Linkul de resetare este invalid sau a expirat.");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldChangePasswordAndRevokeAllActiveRefreshTokens()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        var userToken =
            CreatePasswordResetToken(
                user,
                "RESET_HASH");

        var activeToken1 =
            CreateRefreshToken(
                user,
                "REFRESH_1");

        var activeToken2 =
            CreateRefreshToken(
                user,
                "REFRESH_2");

        var alreadyRevoked =
            CreateRefreshToken(
                user,
                "REFRESH_3");

        alreadyRevoked.RevokedAt =
            DateTime.UtcNow.AddDays(-1);

        dbContext.Users.Add(user);

        dbContext.UserTokens.Add(
            userToken);

        dbContext.RefreshTokens.AddRange(
            activeToken1,
            activeToken2,
            alreadyRevoked);

        await dbContext.SaveChangesAsync();

        var passwordHasher =
            new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(x =>
                x.Hash("NewPassword123!"))
            .Returns("NEW_PASSWORD_HASH");

        var tokenGenerator =
            CreateHashingTokenGenerator(
                "RESET_TOKEN",
                "RESET_HASH");

        var service =
            CreateService(
                dbContext,
                passwordHasher,
                tokenGenerator);

        await service.ResetPasswordAsync(
            new ResetPasswordRequest
            {
                Token = "RESET_TOKEN",
                Password =
                    "NewPassword123!",
                ConfirmPassword =
                    "NewPassword123!"
            });

        user.PasswordHash.Should()
            .Be("NEW_PASSWORD_HASH");

        userToken.UsedAt.Should()
            .NotBeNull();

        activeToken1.RevokedAt.Should()
            .NotBeNull();

        activeToken2.RevokedAt.Should()
            .NotBeNull();

        alreadyRevoked.RevokedAt.Should()
            .NotBeNull();
    }

    // =========================================================
    // CONFIGURATION DEFAULTS
    // =========================================================

    [Fact]
    public async Task LoginAsync_ShouldUseDefaultExpirationValues_WhenConfigurationValuesAreInvalid()
    {
        await using var dbContext =
            CreateDbContext();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var passwordHasher =
            new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(x =>
                x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
            .Returns(true);

        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x => x.GenerateToken())
            .Returns("REFRESH");

        tokenGenerator
            .Setup(x =>
                x.HashToken("REFRESH"))
            .Returns("REFRESH_HASH");

        var jwtService =
            new Mock<IJwtService>();

        jwtService
            .Setup(x =>
                x.GenerateAccessToken(
                    It.IsAny<JwtUserData>()))
            .Returns("ACCESS");

        var configuration =
            CreateConfiguration(
                accessTokenMinutes: 0,
                refreshTokenDays: 0);

        var service =
            CreateService(
                dbContext,
                passwordHasher,
                tokenGenerator,
                jwtService,
                configuration:
                    configuration);

        var before =
            DateTime.UtcNow;

        var result =
            await service.LoginAsync(
                new LoginRequest
                {
                    Email = user.Email,
                    Password = "password"
                });

        var after =
            DateTime.UtcNow;

        result.AccessTokenExpiresAt.Should()
            .BeOnOrAfter(
                before.AddMinutes(15));

        result.AccessTokenExpiresAt.Should()
            .BeOnOrBefore(
                after.AddMinutes(15));

        var refreshToken =
            await dbContext.RefreshTokens
                .SingleAsync();

        refreshToken.ExpiresAt.Should()
            .BeOnOrAfter(
                before.AddDays(30));

        refreshToken.ExpiresAt.Should()
            .BeOnOrBefore(
                after.AddDays(30));
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static RefreshToken CreateRefreshToken(
        User user,
        string hash)
    {
        var now =
            DateTime.UtcNow;

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = hash,
            CreatedAt = now,
            ExpiresAt =
                now.AddDays(30)
        };
    }

    private static UserToken CreatePasswordResetToken(
        User user,
        string hash)
    {
        var now =
            DateTime.UtcNow;

        return new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,

            Type =
                UserTokenType.PasswordReset,

            TokenHash = hash,

            CreatedAt = now,
            ExpiresAt =
                now.AddHours(1)
        };
    }

    private static Mock<ISecureTokenGenerator>
        CreateHashingTokenGenerator(
            string token,
            string hash)
    {
        var tokenGenerator =
            new Mock<ISecureTokenGenerator>();

        tokenGenerator
            .Setup(x =>
                x.HashToken(token))
            .Returns(hash);

        return tokenGenerator;
    }
}