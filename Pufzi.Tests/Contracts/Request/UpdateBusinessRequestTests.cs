using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FluentAssertions;
using Pufzi.Contracts.Requests.Businesses;

namespace Pufzi.Tests.Contracts.Requests;

public class UpdateBusinessRequestTests
{
    [Fact]
    public void ValidRequest_ShouldPassValidation()
    {
        var request = CreateValidRequest();

        Validate(request)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void MinimalValidRequest_ShouldPassValidation()
    {
        var request = new UpdateBusinessRequest
        {
            Name = "Pufzi"
        };

        Validate(request)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void EmptyName_ShouldFailValidation()
    {
        var request = CreateValidRequest(
            name: "");

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.Name));
    }

    [Fact]
    public void NameLongerThan200Characters_ShouldFailValidation()
    {
        var request = CreateValidRequest(
            name: new string('A', 201));

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.Name));
    }

    [Fact]
    public void NameWithExactly200Characters_ShouldPassValidation()
    {
        var request = CreateValidRequest(
            name: new string('A', 200));

        Validate(request)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void DescriptionWithExactly2000Characters_ShouldPassValidation()
    {
        var request = CreateValidRequest(
            description: new string('A', 2000));

        Validate(request)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void DescriptionLongerThan2000Characters_ShouldFailValidation()
    {
        var request = CreateValidRequest(
            description: new string('A', 2001));

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.Description));
    }

    [Fact]
    public void PhoneNumberWithExactly30Characters_ShouldPassValidation()
    {
        var request = CreateValidRequest(
            phoneNumber: new string('1', 30));

        Validate(request)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void PhoneNumberLongerThan30Characters_ShouldFailValidation()
    {
        var request = CreateValidRequest(
            phoneNumber: new string('1', 31));

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.PhoneNumber));
    }

    [Theory]
    [InlineData("contact@pufzi.ro")]
    [InlineData("test.user@gmail.com")]
    [InlineData("hello+business@example.com")]
    public void ValidContactEmail_ShouldPassValidation(
        string email)
    {
        var request = CreateValidRequest(
            contactEmail: email);

        Validate(request)
            .Should()
            .BeEmpty();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@pufzi.ro")]
    [InlineData("test@")]
    public void InvalidContactEmail_ShouldFailValidation(
        string email)
    {
        var request = CreateValidRequest(
            contactEmail: email);

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.ContactEmail));
    }

    [Fact]
    public void ContactEmailLongerThan320Characters_ShouldFailValidation()
    {
        var email =
            $"{new string('a', 310)}@example.com";

        var request = CreateValidRequest(
            contactEmail: email);

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.ContactEmail));
    }

    [Theory]
    [InlineData("-90")]
    [InlineData("0")]
    [InlineData("90")]
    public void ValidLatitude_ShouldPassValidation(
        string value)
    {
        var latitude = ParseDecimal(value);

        var request = CreateValidRequest(
            latitude: latitude);

        Validate(request)
            .Should()
            .BeEmpty();
    }

    [Theory]
    [InlineData("-90.1")]
    [InlineData("90.1")]
    public void InvalidLatitude_ShouldFailValidation(
        string value)
    {
        var latitude = ParseDecimal(value);

        var request = CreateValidRequest(
            latitude: latitude);

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.Latitude));
    }

    [Theory]
    [InlineData("-180")]
    [InlineData("0")]
    [InlineData("180")]
    public void ValidLongitude_ShouldPassValidation(
        string value)
    {
        var longitude = ParseDecimal(value);

        var request = CreateValidRequest(
            longitude: longitude);

        Validate(request)
            .Should()
            .BeEmpty();
    }

    [Theory]
    [InlineData("-180.1")]
    [InlineData("180.1")]
    public void InvalidLongitude_ShouldFailValidation(
        string value)
    {
        var longitude = ParseDecimal(value);

        var request = CreateValidRequest(
            longitude: longitude);

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.Longitude));
    }

    [Theory]
    [InlineData("https://instagram.com/pufzi")]
    [InlineData("https://www.instagram.com/pufzi")]
    [InlineData("https://facebook.com/pufzi")]
    public void ValidUrls_ShouldPassValidation(
        string url)
    {
        var request = CreateValidRequest(
            instagramUrl: url,
            facebookUrl: url);

        Validate(request)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void InvalidInstagramUrl_ShouldFailValidation()
    {
        var request = CreateValidRequest(
            instagramUrl: "not-a-url");

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.InstagramUrl));
    }

    [Fact]
    public void InvalidFacebookUrl_ShouldFailValidation()
    {
        var request = CreateValidRequest(
            facebookUrl: "not-a-url");

        AssertInvalid(
            request,
            nameof(UpdateBusinessRequest.FacebookUrl));
    }

    [Theory]
    [InlineData("AddressLine", 301)]
    [InlineData("City", 101)]
    [InlineData("County", 101)]
    [InlineData("PostalCode", 21)]
    [InlineData("InstagramUrl", 501)]
    [InlineData("FacebookUrl", 501)]
    public void PropertyExceedingMaximumLength_ShouldFailValidation(
        string property,
        int length)
    {
        var value = new string('A', length);

        var request = new UpdateBusinessRequest
        {
            Name = "Pufzi",
            AddressLine =
                property == "AddressLine"
                    ? value
                    : null,
            City =
                property == "City"
                    ? value
                    : null,
            County =
                property == "County"
                    ? value
                    : null,
            PostalCode =
                property == "PostalCode"
                    ? value
                    : null,
            InstagramUrl =
                property == "InstagramUrl"
                    ? value
                    : null,
            FacebookUrl =
                property == "FacebookUrl"
                    ? value
                    : null
        };

        AssertInvalid(
            request,
            property);
    }

    private static UpdateBusinessRequest CreateValidRequest(
        string name = "Pufzi Grooming",
        string? description = "Salon pentru animale.",
        string? phoneNumber = "0712345678",
        string? contactEmail = "contact@pufzi.ro",
        decimal? latitude = 47.0465m,
        decimal? longitude = 21.9189m,
        string? instagramUrl =
            "https://instagram.com/pufzi",
        string? facebookUrl =
            "https://facebook.com/pufzi")
    {
        return new UpdateBusinessRequest
        {
            Name = name,
            Description = description,
            PhoneNumber = phoneNumber,
            ContactEmail = contactEmail,
            AddressLine = "Strada Test 1",
            City = "Oradea",
            County = "Bihor",
            PostalCode = "410001",
            Latitude = latitude,
            Longitude = longitude,
            InstagramUrl = instagramUrl,
            FacebookUrl = facebookUrl
        };
    }

    private static void AssertInvalid(
        UpdateBusinessRequest request,
        string propertyName)
    {
        var results = Validate(request);

        results.Should()
            .Contain(x =>
                x.MemberNames.Contains(
                    propertyName));
    }

    private static List<ValidationResult> Validate(
        object instance)
    {
        var results =
            new List<ValidationResult>();

        Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true);

        return results;
    }

    private static decimal ParseDecimal(
        string value)
    {
        return decimal.Parse(
            value,
            CultureInfo.InvariantCulture);
    }
}