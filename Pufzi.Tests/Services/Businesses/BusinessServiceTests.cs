using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pufzi.Contracts.Requests.Businesses;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Infrastructure.Storage;
using Pufzi.Services.Businesses;

namespace Pufzi.Tests.Services.Businesses;

public class BusinessServiceTests
{
    private static PufziDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<PufziDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new PufziDbContext(options);
    }

    private static Mock<IBlobStorageService>
        CreateBlobStorageMock()
    {
        var mock =
            new Mock<IBlobStorageService>();

        mock
            .Setup(x =>
                x.UploadPublicAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (Stream _,
                    string blobName,
                    string _,
                    CancellationToken _) =>
                    $"http://storage/{blobName}");

        mock
            .Setup(x =>
                x.GetPublicUrl(
                    It.IsAny<string>()))
            .Returns(
                (string blobName) =>
                    $"http://storage/{blobName}");

        mock
            .Setup(x =>
                x.DeletePublicAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    private static User CreateUser(
        Guid? id = null,
        bool isActive = true)
    {
        var user =
            Activator.CreateInstance<User>()!;

        user.Id = id ?? Guid.NewGuid();
        user.IsActive = isActive;

        SetPropertyIfExists(
            user,
            "FirstName",
            "Test");

        SetPropertyIfExists(
            user,
            "LastName",
            "User");

        SetPropertyIfExists(
            user,
            "Email",
            $"test-{Guid.NewGuid():N}@pufzi.ro");

        SetPropertyIfExists(
            user,
            "NormalizedEmail",
            $"TEST-{Guid.NewGuid():N}@PUFZI.RO");

        SetPropertyIfExists(
            user,
            "PasswordHash",
            "test-password-hash");

        SetPropertyIfExists(
            user,
            "EmailConfirmed",
            true);

        SetPropertyIfExists(
            user,
            "CreatedAt",
            DateTime.UtcNow);

        SetPropertyIfExists(
            user,
            "UpdatedAt",
            DateTime.UtcNow);

        return user;
    }

    private static void SetPropertyIfExists(
        object target,
        string propertyName,
        object? value)
    {
        var property =
            target.GetType()
                .GetProperty(propertyName);

        if (property is null ||
            !property.CanWrite)
        {
            return;
        }

        property.SetValue(
            target,
            value);
    }

    private static Business CreateBusiness(
        Guid? id = null,
        string name = "Pufzi Grooming",
        string slug = "pufzi-grooming")
    {
        var now =
            DateTime.UtcNow;

        return new Business
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Slug = slug,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static BusinessMembership CreateMembership(
        Guid userId,
        Guid businessId,
        BusinessRole role = BusinessRole.Owner,
        bool isActive = true)
    {
        var now =
            DateTime.UtcNow;

        return new BusinessMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BusinessId = businessId,
            Role = role,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateBusinessAndOwnerMembership()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new CreateBusinessRequest
            {
                Name = "Pufzi Grooming",
                Description = "Salon pentru animale",
                PhoneNumber = "0750000000",
                ContactEmail = "salon@pufzi.ro",
                AddressLine = "Strada Test 1",
                City = "Oradea",
                County = "Bihor",
                PostalCode = "410001",
                Latitude = 47.0465m,
                Longitude = 21.9189m,
                InstagramUrl =
                    "https://instagram.com/pufzi",
                FacebookUrl =
                    "https://facebook.com/pufzi"
            };

        var result =
            await service.CreateAsync(
                user.Id,
                request);

        result.Should().NotBeNull();
        result.Name.Should()
            .Be("Pufzi Grooming");
        result.Slug.Should()
            .Be("pufzi-grooming");
        result.CurrentUserRole.Should()
            .Be("Owner");

        var business =
            await dbContext.Businesses
                .SingleAsync();

        business.Name.Should()
            .Be("Pufzi Grooming");

        var membership =
            await dbContext.BusinessMemberships
                .SingleAsync();

        membership.UserId.Should()
            .Be(user.Id);

        membership.BusinessId.Should()
            .Be(business.Id);

        membership.Role.Should()
            .Be(BusinessRole.Owner);

        membership.IsActive.Should()
            .BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimAndCleanValues()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new CreateBusinessRequest
            {
                Name = "   Salon Pufzi   ",
                Description = "   Descriere   ",
                PhoneNumber = "   ",
                ContactEmail = "  contact@pufzi.ro  ",
                AddressLine = "",
                City = "   Oradea   ",
                County = "   Bihor   ",
                PostalCode = "   ",
                InstagramUrl = "   ",
                FacebookUrl = null
            };

        await service.CreateAsync(
            user.Id,
            request);

        var business =
            await dbContext.Businesses
                .SingleAsync();

        business.Name.Should()
            .Be("Salon Pufzi");

        business.Description.Should()
            .Be("Descriere");

        business.PhoneNumber.Should()
            .BeNull();

        business.ContactEmail.Should()
            .Be("contact@pufzi.ro");

        business.AddressLine.Should()
            .BeNull();

        business.City.Should()
            .Be("Oradea");

        business.County.Should()
            .Be("Bihor");

        business.PostalCode.Should()
            .BeNull();

        business.InstagramUrl.Should()
            .BeNull();

        business.FacebookUrl.Should()
            .BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameIsEmpty()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new CreateBusinessRequest
            {
                Name = "   "
            };

        var action =
            async () =>
                await service.CreateAsync(
                    user.Id,
                    request);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Numele salonului este obligatoriu.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new CreateBusinessRequest
            {
                Name = "Pufzi"
            };

        var action =
            async () =>
                await service.CreateAsync(
                    Guid.NewGuid(),
                    request);

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Utilizatorul nu este valid.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserIsInactive()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser(
                isActive: false);

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new CreateBusinessRequest
            {
                Name = "Pufzi"
            };

        var action =
            async () =>
                await service.CreateAsync(
                    user.Id,
                    request);

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Utilizatorul nu este valid.");
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateSlugWithoutRomanianDiacritics()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new CreateBusinessRequest
            {
                Name =
                    "Cățeluș Și Frumușel În Orășel"
            };

        var result =
            await service.CreateAsync(
                user.Id,
                request);

        result.Slug.Should()
            .Be(
                "catelus-si-frumusel-in-orasel");
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateUniqueSlug_WhenSlugAlreadyExists()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        dbContext.Businesses.AddRange(
            CreateBusiness(
                name: "Pufzi",
                slug: "pufzi"),
            CreateBusiness(
                name: "Pufzi",
                slug: "pufzi-2"));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new CreateBusinessRequest
            {
                Name = "Pufzi"
            };

        var result =
            await service.CreateAsync(
                user.Id,
                request);

        result.Slug.Should()
            .Be("pufzi-3");
    }

    [Fact]
    public async Task CreateAsync_ShouldUseSalonSlug_WhenNameContainsNoValidCharacters()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new CreateBusinessRequest
            {
                Name = "!!!"
            };

        var result =
            await service.CreateAsync(
                user.Id,
                request);

        result.Slug.Should()
            .Be("salon");
    }

    [Fact]
    public async Task GetMineAsync_ShouldReturnOnlyActiveMemberships()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var activeBusiness =
            CreateBusiness(
                name: "Active",
                slug: "active");

        var inactiveBusiness =
            CreateBusiness(
                name: "Inactive",
                slug: "inactive");

        dbContext.Users.Add(user);

        dbContext.Businesses.AddRange(
            activeBusiness,
            inactiveBusiness);

        dbContext.BusinessMemberships.AddRange(
            CreateMembership(
                user.Id,
                activeBusiness.Id,
                BusinessRole.Owner,
                true),
            CreateMembership(
                user.Id,
                inactiveBusiness.Id,
                BusinessRole.Owner,
                false));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var result =
            await service.GetMineAsync(
                user.Id);

        result.Should()
            .HaveCount(1);

        result.Single().Name.Should()
            .Be("Active");
    }

    [Fact]
    public async Task GetMineAsync_ShouldMapImagesOrderedBySortOrder()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        dbContext.BusinessImages.AddRange(
            new BusinessImage
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                BlobName = "image-2.jpg",
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new BusinessImage
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                BlobName = "image-0.jpg",
                SortOrder = 0,
                CreatedAt = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var result =
            await service.GetMineAsync(
                user.Id);

        var response =
            result.Single();

        response.Images.Should()
    .HaveCount(2);

        var images = response.Images.ToList();

        images[0]
            .SortOrder.Should()
            .Be(0);

        images[1]
            .SortOrder.Should()
            .Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBusiness_WhenMembershipExists()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id,
                BusinessRole.Groomer));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var result =
            await service.GetByIdAsync(
                user.Id,
                business.Id);

        result.Id.Should()
            .Be(business.Id);

        result.CurrentUserRole.Should()
            .Be("Groomer");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenMembershipDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var action =
            async () =>
                await service.GetByIdAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid());

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Nu ai acces la acest salon.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBusiness_WhenUserIsOwner()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id,
                BusinessRole.Owner));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new UpdateBusinessRequest
            {
                Name = "   Salon Nou   ",
                Description = "   Descriere nouă   ",
                PhoneNumber = "   0755555555   ",
                ContactEmail =
                    "   contact@salon.ro   ",
                City = "   Cluj-Napoca   ",
                County = "   Cluj   "
            };

        var result =
            await service.UpdateAsync(
                user.Id,
                business.Id,
                request);

        result.Name.Should()
            .Be("Salon Nou");

        result.Description.Should()
            .Be("Descriere nouă");

        result.PhoneNumber.Should()
            .Be("0755555555");

        result.ContactEmail.Should()
            .Be("contact@salon.ro");

        result.City.Should()
            .Be("Cluj-Napoca");

        result.County.Should()
            .Be("Cluj");

        result.Slug.Should()
            .Be("pufzi-grooming");
    }

    [Fact]
    public async Task UpdateAsync_ShouldConvertWhitespaceOptionalValuesToNull()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        business.Description =
            "Old description";

        business.PhoneNumber =
            "0711111111";

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new UpdateBusinessRequest
            {
                Name = "Pufzi",
                Description = "   ",
                PhoneNumber = "",
                ContactEmail = null
            };

        var result =
            await service.UpdateAsync(
                user.Id,
                business.Id,
                request);

        result.Description.Should()
            .BeNull();

        result.PhoneNumber.Should()
            .BeNull();

        result.ContactEmail.Should()
            .BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenNameIsEmpty()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new UpdateBusinessRequest
            {
                Name = "   "
            };

        var action =
            async () =>
                await service.UpdateAsync(
                    user.Id,
                    business.Id,
                    request);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Numele salonului este obligatoriu.");
    }

    [Theory]
    [InlineData(BusinessRole.Groomer)]
    [InlineData(BusinessRole.Receptionist)]
    public async Task UpdateAsync_ShouldThrow_WhenUserIsNotOwner(
        BusinessRole role)
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id,
                role));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var request =
            new UpdateBusinessRequest
            {
                Name = "Test"
            };

        var action =
            async () =>
                await service.UpdateAsync(
                    user.Id,
                    business.Id,
                    request);

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Doar proprietarul salonului poate efectua această acțiune.");
    }

    [Fact]
    public async Task UploadLogoAsync_ShouldUploadAndSaveLogo()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await using var stream =
            new MemoryStream(
                [1, 2, 3]);

        var result =
            await service.UploadLogoAsync(
                user.Id,
                business.Id,
                stream,
                "image/jpeg",
                ".jpg");

        var savedBusiness =
            await dbContext.Businesses
                .SingleAsync();

        savedBusiness.LogoBlobName.Should()
            .NotBeNull();

        savedBusiness.LogoBlobName.Should()
            .StartWith(
                $"businesses/{business.Id}/logo/");

        savedBusiness.LogoBlobName.Should()
            .EndWith(".jpg");

        result.LogoUrl.Should()
            .Be(
                $"http://storage/{savedBusiness.LogoBlobName}");

        blobStorage.Verify(
            x =>
                x.UploadPublicAsync(
                    It.IsAny<Stream>(),
                    It.Is<string>(
                        name =>
                            name.StartsWith(
                                $"businesses/{business.Id}/logo/") &&
                            name.EndsWith(".jpg")),
                    "image/jpeg",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UploadLogoAsync_ShouldDeleteOldLogo_WhenLogoAlreadyExists()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        business.LogoBlobName =
            "businesses/old-logo.jpg";

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await using var stream =
            new MemoryStream(
                [1, 2, 3]);

        await service.UploadLogoAsync(
            user.Id,
            business.Id,
            stream,
            "image/png",
            ".png");

        blobStorage.Verify(
            x =>
                x.DeletePublicAsync(
                    "businesses/old-logo.jpg",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteLogoAsync_ShouldRemoveLogoAndDeleteBlob()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        business.LogoBlobName =
            "businesses/logo.jpg";

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await service.DeleteLogoAsync(
            user.Id,
            business.Id);

        business.LogoBlobName.Should()
            .BeNull();

        blobStorage.Verify(
            x =>
                x.DeletePublicAsync(
                    "businesses/logo.jpg",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteLogoAsync_ShouldDoNothing_WhenLogoDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await service.DeleteLogoAsync(
            user.Id,
            business.Id);

        blobStorage.Verify(
            x =>
                x.DeletePublicAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadCoverAsync_ShouldUploadAndSaveCover()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await using var stream =
            new MemoryStream(
                [1, 2, 3]);

        var result =
            await service.UploadCoverAsync(
                user.Id,
                business.Id,
                stream,
                "image/webp",
                ".webp");

        business.CoverImageBlobName.Should()
            .NotBeNull();

        business.CoverImageBlobName.Should()
            .StartWith(
                $"businesses/{business.Id}/cover/");

        business.CoverImageBlobName.Should()
            .EndWith(".webp");

        result.CoverImageUrl.Should()
            .Be(
                $"http://storage/{business.CoverImageBlobName}");
    }

    [Fact]
    public async Task UploadCoverAsync_ShouldDeleteOldCover_WhenCoverExists()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        business.CoverImageBlobName =
            "businesses/old-cover.jpg";

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await using var stream =
            new MemoryStream(
                [1, 2, 3]);

        await service.UploadCoverAsync(
            user.Id,
            business.Id,
            stream,
            "image/jpeg",
            ".jpg");

        blobStorage.Verify(
            x =>
                x.DeletePublicAsync(
                    "businesses/old-cover.jpg",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCoverAsync_ShouldRemoveCoverAndDeleteBlob()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        business.CoverImageBlobName =
            "businesses/cover.jpg";

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await service.DeleteCoverAsync(
            user.Id,
            business.Id);

        business.CoverImageBlobName.Should()
            .BeNull();

        blobStorage.Verify(
            x =>
                x.DeletePublicAsync(
                    "businesses/cover.jpg",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCoverAsync_ShouldDoNothing_WhenCoverDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await service.DeleteCoverAsync(
            user.Id,
            business.Id);

        blobStorage.Verify(
            x =>
                x.DeletePublicAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddImageAsync_ShouldCreateFirstImageWithSortOrderZero()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await using var stream =
            new MemoryStream(
                [1, 2, 3]);

        var result =
            await service.AddImageAsync(
                user.Id,
                business.Id,
                stream,
                "image/jpeg",
                ".jpg");

        result.SortOrder.Should()
            .Be(0);

        result.Url.Should()
            .StartWith(
                "http://storage/businesses/");

        var image =
            await dbContext.BusinessImages
                .SingleAsync();

        image.SortOrder.Should()
            .Be(0);

        image.BusinessId.Should()
            .Be(business.Id);
    }

    [Fact]
    public async Task AddImageAsync_ShouldUseNextSortOrder()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        dbContext.BusinessImages.AddRange(
            new BusinessImage
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                BlobName = "image-0.jpg",
                SortOrder = 0,
                CreatedAt = DateTime.UtcNow
            },
            new BusinessImage
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                BlobName = "image-4.jpg",
                SortOrder = 4,
                CreatedAt = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await using var stream =
            new MemoryStream(
                [1]);

        var result =
            await service.AddImageAsync(
                user.Id,
                business.Id,
                stream,
                "image/png",
                ".png");

        result.SortOrder.Should()
            .Be(5);
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldDeleteImageFromDatabaseAndStorage()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        var image =
            new BusinessImage
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                BlobName =
                    "businesses/gallery/image.jpg",
                SortOrder = 0,
                CreatedAt = DateTime.UtcNow
            };

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        dbContext.BusinessImages.Add(image);

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await service.DeleteImageAsync(
            user.Id,
            business.Id,
            image.Id);

        var exists =
            await dbContext.BusinessImages
                .AnyAsync(
                    x => x.Id == image.Id);

        exists.Should()
            .BeFalse();

        blobStorage.Verify(
            x =>
                x.DeletePublicAsync(
                    "businesses/gallery/image.jpg",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldThrow_WhenImageDoesNotExist()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var action =
            async () =>
                await service.DeleteImageAsync(
                    user.Id,
                    business.Id,
                    Guid.NewGuid());

        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage(
                "Imaginea nu a fost găsită.");
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldThrow_WhenImageBelongsToAnotherBusiness()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        var anotherBusiness =
            CreateBusiness(
                name: "Another",
                slug: "another");

        var image =
            new BusinessImage
            {
                Id = Guid.NewGuid(),
                BusinessId =
                    anotherBusiness.Id,
                BlobName = "other.jpg",
                SortOrder = 0,
                CreatedAt = DateTime.UtcNow
            };

        dbContext.Users.Add(user);

        dbContext.Businesses.AddRange(
            business,
            anotherBusiness);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        dbContext.BusinessImages.Add(image);

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var action =
            async () =>
                await service.DeleteImageAsync(
                    user.Id,
                    business.Id,
                    image.Id);

        await action.Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Theory]
    [InlineData(BusinessRole.Groomer)]
    [InlineData(BusinessRole.Receptionist)]
    public async Task UploadLogoAsync_ShouldThrow_WhenUserIsNotOwner(
        BusinessRole role)
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id,
                role));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        await using var stream =
            new MemoryStream([1]);

        var action =
            async () =>
                await service.UploadLogoAsync(
                    user.Id,
                    business.Id,
                    stream,
                    "image/jpeg",
                    ".jpg");

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(
                "Doar proprietarul salonului poate efectua această acțiune.");

        blobStorage.Verify(
            x =>
                x.UploadPublicAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldMapLogoAndCoverUrls()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        business.LogoBlobName =
            "logo.jpg";

        business.CoverImageBlobName =
            "cover.jpg";

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var result =
            await service.GetByIdAsync(
                user.Id,
                business.Id);

        result.LogoUrl.Should()
            .Be("http://storage/logo.jpg");

        result.CoverImageUrl.Should()
            .Be("http://storage/cover.jpg");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullImageUrls_WhenNoImagesExist()
    {
        await using var dbContext =
            CreateDbContext();

        var blobStorage =
            CreateBlobStorageMock();

        var user =
            CreateUser();

        var business =
            CreateBusiness();

        dbContext.Users.Add(user);
        dbContext.Businesses.Add(business);

        dbContext.BusinessMemberships.Add(
            CreateMembership(
                user.Id,
                business.Id));

        await dbContext.SaveChangesAsync();

        var service =
            new BusinessService(
                dbContext,
                blobStorage.Object);

        var result =
            await service.GetByIdAsync(
                user.Id,
                business.Id);

        result.LogoUrl.Should()
            .BeNull();

        result.CoverImageUrl.Should()
            .BeNull();
    }
}