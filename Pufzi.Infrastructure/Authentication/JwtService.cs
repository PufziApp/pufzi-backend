using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Pufzi.Infrastructure.Authentication;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateAccessToken(JwtUserData userData)
    {
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                userData.UserId.ToString()),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                userData.Email),

            new(
                "email_confirmed",
                userData.EmailConfirmed
                    .ToString()
                    .ToLowerInvariant()),

            new(
                "platform_role",
                userData.PlatformRole)
        };

        if (userData.BusinessId.HasValue)
        {
            claims.Add(
                new Claim(
                    "business_id",
                    userData.BusinessId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(
                userData.BusinessRole))
        {
            claims.Add(
                new Claim(
                    "business_role",
                    userData.BusinessRole));
        }

        var signingKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _options.SigningKey));

        var credentials =
            new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(
                    _options.AccessTokenExpirationMinutes),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}