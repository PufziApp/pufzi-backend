using FluentAssertions;
using Pufzi.Infrastructure.Authentication;

namespace Pufzi.Tests.Infrastructure.Authentication;

public class SecureTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_ShouldReturnToken()
    {
        var generator =
            new SecureTokenGenerator();

        var token =
            generator.GenerateToken();

        token.Should()
            .NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ShouldGenerateDifferentTokens()
    {
        var generator =
            new SecureTokenGenerator();

        var firstToken =
            generator.GenerateToken();

        var secondToken =
            generator.GenerateToken();

        firstToken.Should()
            .NotBe(secondToken);
    }

    [Fact]
    public void GenerateToken_ShouldBeUrlSafe()
    {
        var generator =
            new SecureTokenGenerator();

        for (var i = 0; i < 20; i++)
        {
            var token =
                generator.GenerateToken();

            token.Should().NotContain("+");
            token.Should().NotContain("/");
            token.Should().NotContain("=");
        }
    }

    [Fact]
    public void HashToken_ShouldReturnSameHash_ForSameToken()
    {
        var generator =
            new SecureTokenGenerator();

        const string token =
            "test-token";

        var firstHash =
            generator.HashToken(token);

        var secondHash =
            generator.HashToken(token);

        firstHash.Should()
            .Be(secondHash);
    }

    [Fact]
    public void HashToken_ShouldReturnDifferentHashes_ForDifferentTokens()
    {
        var generator =
            new SecureTokenGenerator();

        var firstHash =
            generator.HashToken("token-one");

        var secondHash =
            generator.HashToken("token-two");

        firstHash.Should()
            .NotBe(secondHash);
    }

    [Fact]
    public void HashToken_ShouldNotContainOriginalToken()
    {
        var generator =
            new SecureTokenGenerator();

        const string token =
            "very-secret-refresh-token";

        var hash =
            generator.HashToken(token);

        hash.Should().NotBe(token);
        hash.Should().NotContain(token);
    }

    [Fact]
    public void HashToken_ShouldReturnSha256HexLength()
    {
        var generator =
            new SecureTokenGenerator();

        var hash =
            generator.HashToken("test");

        hash.Should().HaveLength(64);
    }
}