using System.Security.Cryptography;
using System.Text;

namespace Pufzi.Infrastructure.Security;

public class SecureTokenService : ISecureTokenService
{
    private const int TokenSizeInBytes = 32;

    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}