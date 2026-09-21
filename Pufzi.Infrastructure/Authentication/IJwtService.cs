namespace Pufzi.Infrastructure.Authentication;

public interface IJwtService
{
    string GenerateAccessToken(JwtUserData userData);
}