using FluentAssertions;
using Pufzi.Infrastructure.Authentication;

namespace Pufzi.Tests.Infrastructure.Authentication;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ShouldNotReturnPlainPassword()
    {
        var hasher = new PasswordHasher();

        const string password =
            "SuperSecretPassword123!";

        var hash = hasher.Hash(password);

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordIsCorrect()
    {
        var hasher = new PasswordHasher();

        const string password =
            "SuperSecretPassword123!";

        var hash = hasher.Hash(password);

        var result = hasher.Verify(
            password,
            hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        var hasher = new PasswordHasher();

        var hash = hasher.Hash(
            "CorrectPassword123!");

        var result = hasher.Verify(
            "WrongPassword123!",
            hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        var hasher = new PasswordHasher();

        const string password =
            "SamePassword123!";

        var firstHash =
            hasher.Hash(password);

        var secondHash =
            hasher.Hash(password);

        firstHash.Should()
            .NotBe(secondHash);

        hasher.Verify(password, firstHash)
            .Should().BeTrue();

        hasher.Verify(password, secondHash)
            .Should().BeTrue();
    }
}