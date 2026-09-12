using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pufzi.Contracts.Requests.Auth;
using Pufzi.Contracts.Responses.Auth;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Infrastructure.Authentication;
using Pufzi.Infrastructure.Email;
using Pufzi.Services.EmailTemplates;

namespace Pufzi.Services.Auth;

public class AuthService : IAuthService
{
    private readonly PufziDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(
        PufziDbContext dbContext,
        IPasswordHasher passwordHasher,
        ISecureTokenGenerator tokenGenerator,
        IJwtService jwtService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException(
                "Parolele nu coincid.");
        }

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var email = request.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();

        var emailExists = await _dbContext.Users
            .AnyAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "Există deja un cont cu această adresă de email.");
        }

        var now = DateTime.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),

            PlatformRole = PlatformRole.User,

            EmailConfirmed = false,
            IsActive = true,

            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Users.Add(user);

        var confirmationToken =
            _tokenGenerator.GenerateToken();

        var userToken = new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,

            Type = UserTokenType.EmailConfirmation,

            TokenHash =
                _tokenGenerator.HashToken(confirmationToken),

            CreatedAt = now,
            ExpiresAt = now.AddHours(24)
        };

        _dbContext.UserTokens.Add(userToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var frontendUrl = GetFrontendUrl();

        var confirmationUrl =
            $"{frontendUrl}/confirm-email?token=" +
            Uri.EscapeDataString(confirmationToken);

        var htmlContent =
            AuthEmailTemplates.EmailConfirmation(
                user.FirstName,
                confirmationUrl);

        await _emailService.SendAsync(
            user.Email,
            "Confirmă contul Pufzi",
            htmlContent,
            cancellationToken);

        return new RegisterResponse
        {
            Message =
                "Contul a fost creat. Verifică emailul pentru confirmare."
        };
    }

    public async Task ConfirmEmailAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Tokenul de confirmare este invalid.");
        }

        var tokenHash =
            _tokenGenerator.HashToken(token);

        var userToken = await _dbContext.UserTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x =>
                    x.TokenHash == tokenHash &&
                    x.Type == UserTokenType.EmailConfirmation,
                cancellationToken);

        if (userToken is null)
        {
            throw new InvalidOperationException(
                "Tokenul de confirmare este invalid.");
        }

        if (userToken.UsedAt.HasValue)
        {
            throw new InvalidOperationException(
                "Acest link de confirmare a fost deja folosit.");
        }

        if (userToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Linkul de confirmare a expirat.");
        }

        var now = DateTime.UtcNow;

        userToken.User.EmailConfirmed = true;
        userToken.User.UpdatedAt = now;
        userToken.UsedAt = now;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (user is null ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !_passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Email sau parolă incorectă.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Contul este dezactivat.");
        }

        if (!user.EmailConfirmed)
        {
            throw new UnauthorizedAccessException(
                "Adresa de email nu a fost confirmată.");
        }

        return await CreateAuthResponseAsync(
            user,
            cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException(
                "Refresh token invalid.");
        }

        var tokenHash =
            _tokenGenerator.HashToken(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null ||
            storedToken.RevokedAt.HasValue ||
            storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(
                "Refresh token invalid sau expirat.");
        }

        if (!storedToken.User.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Contul este dezactivat.");
        }

        if (!storedToken.User.EmailConfirmed)
        {
            throw new UnauthorizedAccessException(
                "Adresa de email nu este confirmată.");
        }

        var now = DateTime.UtcNow;

        var newRefreshToken =
            _tokenGenerator.GenerateToken();

        var newRefreshTokenHash =
            _tokenGenerator.HashToken(newRefreshToken);

        storedToken.RevokedAt = now;
        storedToken.ReplacedByTokenHash =
            newRefreshTokenHash;

        var newStoredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = storedToken.UserId,
            TokenHash = newRefreshTokenHash,
            CreatedAt = now,

            ExpiresAt = now.AddDays(
                GetRefreshTokenExpirationDays())
        };

        _dbContext.RefreshTokens.Add(
            newStoredToken);

        var accessToken =
            GenerateAccessToken(storedToken.User);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,

            AccessTokenExpiresAt =
                now.AddMinutes(
                    GetAccessTokenExpirationMinutes())
        };
    }

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash =
            _tokenGenerator.HashToken(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null ||
            storedToken.RevokedAt.HasValue)
        {
            return;
        }

        storedToken.RevokedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                x =>
                    x.NormalizedEmail == normalizedEmail &&
                    x.IsActive,
                cancellationToken);

        if (user is null)
        {
            return;
        }

        var now = DateTime.UtcNow;

        var resetToken =
            _tokenGenerator.GenerateToken();

        var resetTokenEntity = new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,

            Type = UserTokenType.PasswordReset,

            TokenHash =
                _tokenGenerator.HashToken(resetToken),

            CreatedAt = now,
            ExpiresAt = now.AddHours(1)
        };

        _dbContext.UserTokens.Add(
            resetTokenEntity);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var frontendUrl = GetFrontendUrl();

        var resetUrl =
            $"{frontendUrl}/reset-password?token=" +
            Uri.EscapeDataString(resetToken);

        var htmlContent =
            AuthEmailTemplates.PasswordReset(
                user.FirstName,
                resetUrl);

        await _emailService.SendAsync(
            user.Email,
            "Resetare parolă Pufzi",
            htmlContent,
            cancellationToken);
    }

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException(
                "Parolele nu coincid.");
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new InvalidOperationException(
                "Tokenul este invalid.");
        }

        var tokenHash =
            _tokenGenerator.HashToken(request.Token);

        var userToken = await _dbContext.UserTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x =>
                    x.TokenHash == tokenHash &&
                    x.Type == UserTokenType.PasswordReset,
                cancellationToken);

        if (userToken is null ||
            userToken.UsedAt.HasValue ||
            userToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Linkul de resetare este invalid sau a expirat.");
        }

        var now = DateTime.UtcNow;

        userToken.User.PasswordHash =
            _passwordHasher.Hash(request.Password);

        userToken.User.UpdatedAt = now;
        userToken.UsedAt = now;

        var activeRefreshTokens =
            await _dbContext.RefreshTokens
                .Where(x =>
                    x.UserId == userToken.UserId &&
                    x.RevokedAt == null)
                .ToListAsync(cancellationToken);

        foreach (var activeRefreshToken
                 in activeRefreshTokens)
        {
            activeRefreshToken.RevokedAt = now;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var accessToken =
            GenerateAccessToken(user);

        var refreshToken =
            _tokenGenerator.GenerateToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,

            TokenHash =
                _tokenGenerator.HashToken(refreshToken),

            CreatedAt = now,

            ExpiresAt = now.AddDays(
                GetRefreshTokenExpirationDays())
        };

        _dbContext.RefreshTokens.Add(
            refreshTokenEntity);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,

            AccessTokenExpiresAt =
                now.AddMinutes(
                    GetAccessTokenExpirationMinutes())
        };
    }

    private string GenerateAccessToken(
        User user)
    {
        return _jwtService.GenerateAccessToken(
            new JwtUserData
            {
                UserId = user.Id,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,

                PlatformRole =
                    user.PlatformRole.ToString()
            });
    }

    private string GetFrontendUrl()
    {
        return _configuration["Frontend:WebUrl"]
            ?? throw new InvalidOperationException(
                "Frontend URL is not configured.");
    }

    private int GetAccessTokenExpirationMinutes()
    {
        var minutes =
            _configuration.GetValue<int>(
                "Jwt:AccessTokenExpirationMinutes");

        return minutes > 0
            ? minutes
            : 15;
    }

    private int GetRefreshTokenExpirationDays()
    {
        var days =
            _configuration.GetValue<int>(
                "Jwt:RefreshTokenExpirationDays");

        return days > 0
            ? days
            : 30;
    }
}