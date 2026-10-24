using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Requests.Salons;
using Pufzi.Contracts.Responses.Businesses;
using Pufzi.Contracts.Responses.Salons;
using Pufzi.Data.Database;
using Pufzi.Infrastructure.Storage;

namespace Pufzi.Services.Salons;

public class SalonDiscoveryService : ISalonDiscoveryService
{
    private readonly PufziDbContext _dbContext;
    private readonly IBlobStorageService _blobStorage;

    public SalonDiscoveryService(
        PufziDbContext dbContext,
        IBlobStorageService blobStorage)
    {
        _dbContext = dbContext;
        _blobStorage = blobStorage;
    }

    public async Task<SalonListResponse> GetSalonsAsync(
        SalonSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.Businesses
            .AsNoTracking()
            .Where(x => x.IsPublished);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{EscapeLikePattern(request.Search.Trim())}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Name, search, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City.Trim();

            query = query.Where(x =>
                x.City != null &&
                EF.Functions.ILike(x.City, city));
        }

        if (!string.IsNullOrWhiteSpace(request.County))
        {
            var county = request.County.Trim();

            query = query.Where(x =>
                x.County != null &&
                EF.Functions.ILike(x.County, county));
        }

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            var countryCode = request.CountryCode.Trim().ToUpperInvariant();

            query = query.Where(x =>
                x.CountryCode == countryCode);
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var businesses = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = businesses
            .Select(x => new SalonListItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                City = x.City,
                County = x.County,
                CountryCode = x.CountryCode,

                LogoUrl = GetImageUrl(x.LogoBlobName),

                CoverImageUrl = GetImageUrl(x.CoverImageBlobName)
            })
            .ToList();

        return new SalonListResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize)
        };
    }

    public async Task<SalonDetailResponse> GetSalonByIdAsync(
        Guid salonId,
        CancellationToken cancellationToken = default)
    {
        var business = await _dbContext.Businesses
            .AsNoTracking()
            .Include(x => x.Images)
            .FirstOrDefaultAsync(
                x => x.Id == salonId && x.IsPublished,
                cancellationToken);

        if (business is null)
        {
            throw new KeyNotFoundException(
                "Salonul nu a fost găsit.");
        }

        return new SalonDetailResponse
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
            CountryCode = business.CountryCode,
            TimeZoneId = business.TimeZoneId,
            Latitude = business.Latitude,
            Longitude = business.Longitude,

            LogoUrl = GetImageUrl(business.LogoBlobName),

            CoverImageUrl = GetImageUrl(business.CoverImageBlobName),

            InstagramUrl = business.InstagramUrl,
            FacebookUrl = business.FacebookUrl,

            Images = business.Images
                .OrderBy(x => x.SortOrder)
                .Select(x => new BusinessImageResponse
                {
                    Id = x.Id,
                    Url = _blobStorage.GetPublicUrl(x.BlobName),
                    SortOrder = x.SortOrder
                })
                .ToList()
        };
    }

    private string? GetImageUrl(string? blobName)
    {
        return string.IsNullOrWhiteSpace(blobName)
            ? null
            : _blobStorage.GetPublicUrl(blobName);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}