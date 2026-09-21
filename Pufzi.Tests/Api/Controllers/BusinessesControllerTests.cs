using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pufzi.Api.Controllers;
using Pufzi.Contracts.Requests.Businesses;
using Pufzi.Services.Businesses;

namespace Pufzi.Tests.Api.Controllers;

public class BusinessesControllerTests
{
    private readonly Mock<IBusinessService>
        _businessServiceMock;

    private readonly Guid _userId =
        Guid.NewGuid();

    private readonly Guid _businessId =
        Guid.NewGuid();

    private readonly BusinessesController
        _controller;

    public BusinessesControllerTests()
    {
        _businessServiceMock =
            new Mock<IBusinessService>();

        _controller =
            new BusinessesController(
                _businessServiceMock.Object);

        SetAuthenticatedUser(
            _controller,
            _userId);
    }

    [Fact]
    public async Task Create_ShouldCallService_AndReturnOk()
    {
        var request =
            new CreateBusinessRequest
            {
                Name = "Pufzi Grooming"
            };

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.Create(
                request,
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        _businessServiceMock.Verify(
            x => x.CreateAsync(
                _userId,
                request,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetMine_ShouldCallService_AndReturnOk()
    {
        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.GetMine(
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        _businessServiceMock.Verify(
            x => x.GetMineAsync(
                _userId,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldCallService_AndReturnOk()
    {
        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.GetById(
                _businessId,
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        _businessServiceMock.Verify(
            x => x.GetByIdAsync(
                _userId,
                _businessId,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Update_ShouldCallService_AndReturnOk()
    {
        var request =
            new UpdateBusinessRequest
            {
                Name = "Pufzi Updated"
            };

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.Update(
                _businessId,
                request,
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        _businessServiceMock.Verify(
            x => x.UpdateAsync(
                _userId,
                _businessId,
                request,
                cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData(
        "image/jpeg",
        "test.jpg")]
    [InlineData(
        "image/png",
        "test.png")]
    [InlineData(
        "image/webp",
        "test.webp")]
    public async Task UploadLogo_WithValidImage_ShouldReturnOk(
        string contentType,
        string fileName)
    {
        var file =
            CreateFile(
                contentType,
                fileName);

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.UploadLogo(
                _businessId,
                file,
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        var expectedExtension =
            contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => throw new InvalidOperationException()
            };

        _businessServiceMock.Verify(
            x => x.UploadLogoAsync(
                _userId,
                _businessId,
                It.IsAny<Stream>(),
                contentType,
                expectedExtension,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task UploadLogo_WithEmptyFile_ShouldThrow()
    {
        var file =
            CreateEmptyFile(
                "image/jpeg",
                "empty.jpg");

        var act =
            async () =>
                await _controller.UploadLogo(
                    _businessId,
                    file,
                    CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Fișierul este gol.");

        _businessServiceMock.Verify(
            x => x.UploadLogoAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("text/plain")]
    [InlineData("application/pdf")]
    public async Task UploadLogo_WithInvalidContentType_ShouldThrow(
        string contentType)
    {
        var file =
            CreateFile(
                contentType,
                "file.test");

        var act =
            async () =>
                await _controller.UploadLogo(
                    _businessId,
                    file,
                    CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Sunt permise doar imagini JPEG, PNG sau WEBP.");

        _businessServiceMock.Verify(
            x => x.UploadLogoAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteLogo_ShouldCallService_AndReturnNoContent()
    {
        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.DeleteLogo(
                _businessId,
                cancellationToken);

        result.Should()
            .BeOfType<NoContentResult>();

        _businessServiceMock.Verify(
            x => x.DeleteLogoAsync(
                _userId,
                _businessId,
                cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData(
        "image/jpeg",
        ".jpg")]
    [InlineData(
        "image/png",
        ".png")]
    [InlineData(
        "image/webp",
        ".webp")]
    public async Task UploadCover_WithValidImage_ShouldReturnOk(
        string contentType,
        string expectedExtension)
    {
        var file =
            CreateFile(
                contentType,
                "cover");

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.UploadCover(
                _businessId,
                file,
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        _businessServiceMock.Verify(
            x => x.UploadCoverAsync(
                _userId,
                _businessId,
                It.IsAny<Stream>(),
                contentType,
                expectedExtension,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task UploadCover_WithEmptyFile_ShouldThrow()
    {
        var file =
            CreateEmptyFile(
                "image/jpeg",
                "cover.jpg");

        var act =
            async () =>
                await _controller.UploadCover(
                    _businessId,
                    file,
                    CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Fișierul este gol.");
    }

    [Fact]
    public async Task UploadCover_WithInvalidContentType_ShouldThrow()
    {
        var file =
            CreateFile(
                "application/pdf",
                "document.pdf");

        var act =
            async () =>
                await _controller.UploadCover(
                    _businessId,
                    file,
                    CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Sunt permise doar imagini JPEG, PNG sau WEBP.");
    }

    [Fact]
    public async Task DeleteCover_ShouldCallService_AndReturnNoContent()
    {
        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.DeleteCover(
                _businessId,
                cancellationToken);

        result.Should()
            .BeOfType<NoContentResult>();

        _businessServiceMock.Verify(
            x => x.DeleteCoverAsync(
                _userId,
                _businessId,
                cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData(
        "image/jpeg",
        ".jpg")]
    [InlineData(
        "image/png",
        ".png")]
    [InlineData(
        "image/webp",
        ".webp")]
    public async Task AddImage_WithValidImage_ShouldReturnOk(
        string contentType,
        string expectedExtension)
    {
        var file =
            CreateFile(
                contentType,
                "gallery");

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.AddImage(
                _businessId,
                file,
                cancellationToken);

        result.Should()
            .BeOfType<OkObjectResult>();

        _businessServiceMock.Verify(
            x => x.AddImageAsync(
                _userId,
                _businessId,
                It.IsAny<Stream>(),
                contentType,
                expectedExtension,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task AddImage_WithEmptyFile_ShouldThrow()
    {
        var file =
            CreateEmptyFile(
                "image/png",
                "empty.png");

        var act =
            async () =>
                await _controller.AddImage(
                    _businessId,
                    file,
                    CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Fișierul este gol.");
    }

    [Fact]
    public async Task AddImage_WithInvalidContentType_ShouldThrow()
    {
        var file =
            CreateFile(
                "image/gif",
                "image.gif");

        var act =
            async () =>
                await _controller.AddImage(
                    _businessId,
                    file,
                    CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Sunt permise doar imagini JPEG, PNG sau WEBP.");
    }

    [Fact]
    public async Task DeleteImage_ShouldCallService_AndReturnNoContent()
    {
        var imageId =
            Guid.NewGuid();

        var cancellationToken =
            CancellationToken.None;

        var result =
            await _controller.DeleteImage(
                _businessId,
                imageId,
                cancellationToken);

        result.Should()
            .BeOfType<NoContentResult>();

        _businessServiceMock.Verify(
            x => x.DeleteImageAsync(
                _userId,
                _businessId,
                imageId,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithoutSubClaim_ShouldThrowUnauthorized()
    {
        SetUserWithoutSub(
            _controller);

        var request =
            new CreateBusinessRequest
            {
                Name = "Pufzi"
            };

        var act =
            async () =>
                await _controller.Create(
                    request,
                    CancellationToken.None);

        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Utilizator neautentificat.");
    }

    [Fact]
    public async Task Create_WithInvalidSubClaim_ShouldThrowUnauthorized()
    {
        SetUserWithSub(
            _controller,
            "invalid-guid");

        var request =
            new CreateBusinessRequest
            {
                Name = "Pufzi"
            };

        var act =
            async () =>
                await _controller.Create(
                    request,
                    CancellationToken.None);

        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Utilizator neautentificat.");
    }

    [Fact]
    public void GetExtension_WithUnsupportedContentType_ShouldThrow()
    {
        var file =
            CreateFile(
                "image/gif",
                "image.gif");

        var method =
            typeof(BusinessesController)
                .GetMethod(
                    "GetExtension",
                    BindingFlags.NonPublic |
                    BindingFlags.Static);

        method.Should()
            .NotBeNull();

        var act =
            () => method!.Invoke(
                null,
                new object[]
                {
                    file
                });

        var exception =
            act.Should()
                .Throw<TargetInvocationException>()
                .Which;

        exception.InnerException.Should()
            .BeOfType<InvalidOperationException>()
            .Which.Message.Should()
            .Be("Tip de imagine invalid.");
    }

    private static IFormFile CreateFile(
        string contentType,
        string fileName)
    {
        var bytes =
            new byte[]
            {
                1,
                2,
                3,
                4
            };

        var stream =
            new MemoryStream(bytes);

        return new FormFile(
            stream,
            0,
            stream.Length,
            "file",
            fileName)
        {
            Headers =
                new HeaderDictionary(),

            ContentType =
                contentType
        };
    }

    private static IFormFile CreateEmptyFile(
        string contentType,
        string fileName)
    {
        var stream =
            new MemoryStream();

        return new FormFile(
            stream,
            0,
            0,
            "file",
            fileName)
        {
            Headers =
                new HeaderDictionary(),

            ContentType =
                contentType
        };
    }

    private static void SetAuthenticatedUser(
        ControllerBase controller,
        Guid userId)
    {
        SetUserWithSub(
            controller,
            userId.ToString());
    }

    private static void SetUserWithSub(
        ControllerBase controller,
        string sub)
    {
        var claims =
            new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    sub)
            };

        var identity =
            new ClaimsIdentity(
                claims,
                "TestAuthentication");

        var principal =
            new ClaimsPrincipal(
                identity);

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User = principal
                    }
            };
    }

    private static void SetUserWithoutSub(
        ControllerBase controller)
    {
        var identity =
            new ClaimsIdentity(
                Array.Empty<Claim>(),
                "TestAuthentication");

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User =
                            new ClaimsPrincipal(
                                identity)
                    }
            };
    }
}