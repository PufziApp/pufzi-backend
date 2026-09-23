namespace Pufzi.Infrastructure.Security;

public interface ISecureTokenService
{
    string GenerateToken();

    string HashToken(string token);
}