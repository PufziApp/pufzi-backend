namespace Pufzi.Infrastructure.Authentication;

public interface ISecureTokenGenerator
{
    string GenerateToken();

    string HashToken(string token);
}