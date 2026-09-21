using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Requests.Businesses;
using Pufzi.Contracts.Responses.Businesses;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Infrastructure.Storage;

namespace Pufzi.Services.Businesses;

public class BusinessService : IBusinessService
{
    private readonly PufziDbContext _dbContext;
    private readonly IBlobStorageService _blobStorage;

    public BusinessService(
        PufziDbContext dbContext,
        IBlobStorageService blobStorage)
    {
        _dbContext = dbContext;
        _blobStorage = blobStorage;
    }

    public async Task<BusinessResponse> CreateAsync(
        Guid userId,
        CreateBusinessRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                "Numele salonului este obligatoriu.");
        }

        var userExists = await _dbContext.Users
            .AnyAsync(
                x => x.Id == userId && x.IsActive,
                cancellationToken);

        if (!userExists)
        {
            throw new UnauthorizedAccessException(
                "Utilizatorul nu este valid.");
        }

        var slug = await GenerateUniqueSlugAsync(
            name,
            cancellationToken);

        var now = DateTime.UtcNow;

        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = Clean(request.Description),
            PhoneNumber = Clean(request.PhoneNumber),
            ContactEmail = Clean(request.ContactEmail),
            AddressLine = Clean(request.AddressLine),
            City = Clean(request.City),
            County = Clean(request.County),
            PostalCode = Clean(request.PostalCode),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            InstagramUrl = Clean(request.InstagramUrl),
            FacebookUrl = Clean(request.FacebookUrl),
            CreatedAt = now,
            UpdatedAt = now
        };

        var membership = new BusinessMembership
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            UserId = userId,

            Role = BusinessRole.Owner,

            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Businesses.Add(business);
        _dbContext.BusinessMemberships.Add(membership);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapBusiness(
            business,
            BusinessRole.Owner);
    }

    public async Task<IReadOnlyCollection<BusinessResponse>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var memberships =
            await _dbContext.BusinessMemberships
                .AsNoTracking()
                .Include(x => x.Business)
                    .ThenInclude(x => x.Images)
                .Where(x =>
                    x.UserId == userId &&
                    x.IsActive)
                .ToListAsync(cancellationToken);

        return memberships
            .Select(x =>
                MapBusiness(
                    x.Business,
                    x.Role))
            .ToList();
    }

    public async Task<BusinessResponse> GetByIdAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await GetMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        await _dbContext.Entry(membership.Business)
            .Collection(x => x.Images)
            .LoadAsync(cancellationToken);

        return MapBusiness(
            membership.Business,
            membership.Role);
    }

    public async Task<BusinessResponse> UpdateAsync(
        Guid userId,
        Guid businessId,
        UpdateBusinessRequest request,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await RequireOwnerAsync(
                userId,
                businessId,
                cancellationToken);

        var business = membership.Business;

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                "Numele salonului este obligatoriu.");
        }

        business.Name = name;
        business.Description = Clean(request.Description);
        business.PhoneNumber = Clean(request.PhoneNumber);
        business.ContactEmail = Clean(request.ContactEmail);
        business.AddressLine = Clean(request.AddressLine);
        business.City = Clean(request.City);
        business.County = Clean(request.County);
        business.PostalCode = Clean(request.PostalCode);
        business.Latitude = request.Latitude;
        business.Longitude = request.Longitude;
        business.InstagramUrl = Clean(request.InstagramUrl);
        business.FacebookUrl = Clean(request.FacebookUrl);
        business.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await _dbContext.Entry(business)
            .Collection(x => x.Images)
            .LoadAsync(cancellationToken);

        return MapBusiness(
            business,
            membership.Role);
    }

    public async Task<BusinessResponse> UploadLogoAsync(
        Guid userId,
        Guid businessId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await RequireOwnerAsync(
                userId,
                businessId,
                cancellationToken);

        var business = membership.Business;

        var oldBlobName =
            business.LogoBlobName;

        var blobName =
            $"businesses/{businessId}/logo/{Guid.NewGuid():N}{extension}";

        await _blobStorage.UploadPublicAsync(
            stream,
            blobName,
            contentType,
            cancellationToken);

        business.LogoBlobName = blobName;
        business.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(oldBlobName))
        {
            await _blobStorage.DeletePublicAsync(
                oldBlobName,
                cancellationToken);
        }

        await _dbContext.Entry(business)
            .Collection(x => x.Images)
            .LoadAsync(cancellationToken);

        return MapBusiness(
            business,
            membership.Role);
    }

    public async Task DeleteLogoAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await RequireOwnerAsync(
                userId,
                businessId,
                cancellationToken);

        var business = membership.Business;

        if (string.IsNullOrWhiteSpace(
                business.LogoBlobName))
        {
            return;
        }

        var blobName =
            business.LogoBlobName;

        business.LogoBlobName = null;
        business.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await _blobStorage.DeletePublicAsync(
            blobName,
            cancellationToken);
    }

    public async Task<BusinessResponse> UploadCoverAsync(
        Guid userId,
        Guid businessId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await RequireOwnerAsync(
                userId,
                businessId,
                cancellationToken);

        var business = membership.Business;

        var oldBlobName =
            business.CoverImageBlobName;

        var blobName =
            $"businesses/{businessId}/cover/{Guid.NewGuid():N}{extension}";

        await _blobStorage.UploadPublicAsync(
            stream,
            blobName,
            contentType,
            cancellationToken);

        business.CoverImageBlobName =
            blobName;

        business.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(oldBlobName))
        {
            await _blobStorage.DeletePublicAsync(
                oldBlobName,
                cancellationToken);
        }

        await _dbContext.Entry(business)
            .Collection(x => x.Images)
            .LoadAsync(cancellationToken);

        return MapBusiness(
            business,
            membership.Role);
    }

    public async Task DeleteCoverAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await RequireOwnerAsync(
                userId,
                businessId,
                cancellationToken);

        var business = membership.Business;

        if (string.IsNullOrWhiteSpace(
                business.CoverImageBlobName))
        {
            return;
        }

        var blobName =
            business.CoverImageBlobName;

        business.CoverImageBlobName = null;
        business.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await _blobStorage.DeletePublicAsync(
            blobName,
            cancellationToken);
    }

    public async Task<BusinessImageResponse> AddImageAsync(
        Guid userId,
        Guid businessId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        await RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var blobName =
            $"businesses/{businessId}/gallery/{Guid.NewGuid():N}{extension}";

        await _blobStorage.UploadPublicAsync(
            stream,
            blobName,
            contentType,
            cancellationToken);

        var currentMaxSortOrder =
            await _dbContext.Set<BusinessImage>()
                .Where(x =>
                    x.BusinessId == businessId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync(cancellationToken)
                ?? -1;

        var image = new BusinessImage
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            BlobName = blobName,
            SortOrder =
                currentMaxSortOrder + 1,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Set<BusinessImage>()
            .Add(image);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new BusinessImageResponse
        {
            Id = image.Id,
            Url = _blobStorage.GetPublicUrl(
                image.BlobName),
            SortOrder = image.SortOrder
        };
    }

    public async Task DeleteImageAsync(
        Guid userId,
        Guid businessId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        await RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var image =
            await _dbContext.Set<BusinessImage>()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == imageId &&
                        x.BusinessId == businessId,
                    cancellationToken);

        if (image is null)
        {
            throw new KeyNotFoundException(
                "Imaginea nu a fost găsită.");
        }

        var blobName = image.BlobName;

        _dbContext.Set<BusinessImage>()
            .Remove(image);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await _blobStorage.DeletePublicAsync(
            blobName,
            cancellationToken);
    }

    private async Task<BusinessMembership> GetMembershipAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var membership =
            await _dbContext.BusinessMemberships
                .Include(x => x.Business)
                .FirstOrDefaultAsync(
                    x =>
                        x.UserId == userId &&
                        x.BusinessId == businessId &&
                        x.IsActive,
                    cancellationToken);

        if (membership is null)
        {
            throw new UnauthorizedAccessException(
                "Nu ai acces la acest salon.");
        }

        return membership;
    }

    private async Task<BusinessMembership> RequireOwnerAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var membership =
            await GetMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        if (membership.Role != BusinessRole.Owner)
        {
            throw new UnauthorizedAccessException(
                "Doar proprietarul salonului poate efectua această acțiune.");
        }

        return membership;
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var baseSlug = GenerateSlug(name);

        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "salon";
        }

        var slug = baseSlug;
        var counter = 2;

        while (await _dbContext.Businesses
                   .AnyAsync(
                       x => x.Slug == slug,
                       cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static string GenerateSlug(
        string value)
    {
        var slug =
            value.Trim().ToLowerInvariant();

        slug = slug
            .Replace("ă", "a")
            .Replace("â", "a")
            .Replace("î", "i")
            .Replace("ș", "s")
            .Replace("ş", "s")
            .Replace("ț", "t")
            .Replace("ţ", "t");

        slug = Regex.Replace(
            slug,
            @"[^a-z0-9]+",
            "-");

        return slug.Trim('-');
    }

    private static string? Clean(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private BusinessResponse MapBusiness(
        Business business,
        BusinessRole role)
    {
        return new BusinessResponse
        {
            Id = business.Id,
            Name = business.Name,
            Slug = business.Slug,
            Description = business.Description,
            PhoneNumber = business.PhoneNumber,
            ContactEmail = business.ContactEmail,
            AddressLine = business.AddressLine,
            City = business.City,
            County = business.County,
            PostalCode = business.PostalCode,
            Latitude = business.Latitude,
            Longitude = business.Longitude,

            LogoUrl =
                string.IsNullOrWhiteSpace(
                    business.LogoBlobName)
                    ? null
                    : _blobStorage.GetPublicUrl(
                        business.LogoBlobName),

            CoverImageUrl =
                string.IsNullOrWhiteSpace(
                    business.CoverImageBlobName)
                    ? null
                    : _blobStorage.GetPublicUrl(
                        business.CoverImageBlobName),

            InstagramUrl = business.InstagramUrl,
            FacebookUrl = business.FacebookUrl,

            CurrentUserRole =
                role.ToString(),

            Images = business.Images
                .OrderBy(x => x.SortOrder)
                .Select(x =>
                    new BusinessImageResponse
                    {
                        Id = x.Id,
                        Url =
                            _blobStorage.GetPublicUrl(
                                x.BlobName),
                        SortOrder = x.SortOrder
                    })
                .ToList()
        };
    }
}