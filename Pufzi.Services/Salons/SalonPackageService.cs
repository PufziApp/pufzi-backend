using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Enums;
using Pufzi.Contracts.Requests.ServicePackages;
using Pufzi.Contracts.Responses.ServicePackages;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Infrastructure.Storage;

namespace Pufzi.Services.Salons;

public class SalonPackageService : ISalonPackageService
{
    private readonly PufziDbContext _dbContext;
    private readonly IBlobStorageService _blobStorage;

    public SalonPackageService(
        PufziDbContext dbContext,
        IBlobStorageService blobStorage)
    {
        _dbContext = dbContext;
        _blobStorage = blobStorage;
    }

    public async Task<PagedResponse<ServicePackageResponse>> GetAllAsync(
        Guid salonId,
        GetServicePackagesRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureSalonIsPublishedAsync(
            salonId,
            cancellationToken);

        if (request.MinPrice.HasValue &&
            request.MaxPrice.HasValue &&
            request.MinPrice.Value > request.MaxPrice.Value)
        {
            throw new InvalidOperationException(
                "Prețul minim nu poate fi mai mare decât prețul maxim.");
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.ServicePackages
            .AsNoTracking()
            .Where(x =>
                x.BusinessId == salonId &&
                x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(
                    x.Name,
                    $"%{search}%"));
        }

        if (request.AnimalSpeciesId.HasValue)
        {
            var speciesId = request.AnimalSpeciesId.Value;

            query = query.Where(x =>
                x.AnimalSpecies.Any(s =>
                    s.AnimalSpeciesId == speciesId));
        }

        if (request.EmployeeMembershipId.HasValue)
        {
            var employeeId = request.EmployeeMembershipId.Value;

            query = query.Where(x =>
                x.Employees.Any(e =>
                    e.BusinessMembershipId == employeeId &&
                    e.BusinessMembership.IsActive &&
                    e.BusinessMembership.RemovedAt == null &&
                    e.BusinessMembership.User.IsActive &&
                    e.BusinessMembership.Role != BusinessRole.Receptionist));
        }

        if (request.MinPrice.HasValue)
        {
            var minPrice = request.MinPrice.Value;

            query = query.Where(x =>
                x.Items
                    .SelectMany(i => i.Options)
                    .SelectMany(o => o.Variants)
                    .Any(v =>
                        v.Price.HasValue &&
                        v.Price.Value >= minPrice));
        }

        if (request.MaxPrice.HasValue)
        {
            var maxPrice = request.MaxPrice.Value;

            query = query.Where(x =>
                x.Items
                    .SelectMany(i => i.Options)
                    .SelectMany(o => o.Variants)
                    .Any(v =>
                        v.Price.HasValue &&
                        v.Price.Value <= maxPrice));
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        // Pentru lista publică păstrăm o sortare stabilă.
        // Sortarea avansată după preț poate fi adăugată ulterior.
        query = request.SortBy switch
        {
            ServicePackageSortBy.Name =>
                request.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(x => x.Name)
                        .ThenBy(x => x.Id)
                    : query.OrderBy(x => x.Name)
                        .ThenBy(x => x.Id),

            ServicePackageSortBy.CreatedAt =>
                request.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(x => x.CreatedAt)
                        .ThenBy(x => x.Id)
                    : query.OrderBy(x => x.CreatedAt)
                        .ThenBy(x => x.Id),

            ServicePackageSortBy.Price =>
                request.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(x =>
                        x.Items
                            .SelectMany(i => i.Options)
                            .SelectMany(o => o.Variants)
                            .Where(v => v.Price.HasValue)
                            .Min(v => v.Price))
                        .ThenBy(x => x.Id)
                    : query.OrderBy(x =>
                        x.Items
                            .SelectMany(i => i.Options)
                            .SelectMany(o => o.Variants)
                            .Where(v => v.Price.HasValue)
                            .Min(v => v.Price))
                        .ThenBy(x => x.Id),

            _ =>
                request.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenBy(x => x.Id)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Id)
        };

        var packages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .Include(x => x.AnimalSpecies)
                .ThenInclude(x => x.AnimalSpecies)
            .Include(x => x.Items)
                .ThenInclude(x => x.Options)
                    .ThenInclude(x => x.Variants)
            .Include(x => x.Employees)
                .ThenInclude(x => x.BusinessMembership)
                    .ThenInclude(x => x.User)
            .ToListAsync(cancellationToken);

        return new PagedResponse<ServicePackageResponse>
        {
            Items = packages.Select(MapSummary).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServicePackageDetailsResponse> GetByIdAsync(
        Guid salonId,
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        await EnsureSalonIsPublishedAsync(
            salonId,
            cancellationToken);

        var package = await _dbContext.ServicePackages
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.AnimalSpecies)
                .ThenInclude(x => x.AnimalSpecies)
            .Include(x => x.Employees)
                .ThenInclude(x => x.BusinessMembership)
                    .ThenInclude(x => x.User)
            .Include(x => x.Items)
                .ThenInclude(x => x.Options)
                    .ThenInclude(x => x.AnimalSpecies)
            .Include(x => x.Items)
                .ThenInclude(x => x.Options)
                    .ThenInclude(x => x.Variants)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == packageId &&
                    x.BusinessId == salonId &&
                    x.IsActive,
                cancellationToken);

        if (package is null)
        {
            throw new KeyNotFoundException(
                "Pachetul de servicii nu a fost găsit.");
        }

        return MapDetails(package);
    }

    private async Task EnsureSalonIsPublishedAsync(
        Guid salonId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Businesses
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == salonId && x.IsPublished,
                cancellationToken);

        if (!exists)
        {
            throw new KeyNotFoundException(
                "Salonul nu a fost găsit.");
        }
    }

    private ServicePackageResponse MapSummary(
        ServicePackage package)
    {
        var variants = package.Items
            .SelectMany(x => x.Options)
            .SelectMany(x => x.Variants)
            .ToList();

        return new ServicePackageResponse
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Icon = package.Icon,

            StartingPrice = variants
                .Where(x => x.Price.HasValue)
                .Select(x => x.Price)
                .DefaultIfEmpty(null)
                .Min(),

            HasPriceOnRequest =
                variants.Any(x => x.IsPriceOnRequest),

            IsActive = package.IsActive,
            SortOrder = package.SortOrder,

            AnimalSpecies = package.AnimalSpecies
                .OrderBy(x => x.AnimalSpecies.Code)
                .Select(x => new ServicePackageAnimalSpeciesResponse
                {
                    Id = x.AnimalSpeciesId,
                    Code = x.AnimalSpecies.Code
                })
                .ToList(),

            ItemsCount = package.Items.Count,

            EmployeesCount = package.Employees.Count(x =>
                x.BusinessMembership.IsActive &&
                x.BusinessMembership.RemovedAt == null &&
                x.BusinessMembership.User.IsActive &&
                x.BusinessMembership.Role != BusinessRole.Receptionist),

            CreatedAt = package.CreatedAt,
            UpdatedAt = package.UpdatedAt
        };
    }

    private ServicePackageDetailsResponse MapDetails(
        ServicePackage package)
    {
        return new ServicePackageDetailsResponse
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Icon = package.Icon,
            IsActive = package.IsActive,
            SortOrder = package.SortOrder,

            AnimalSpecies = package.AnimalSpecies
                .OrderBy(x => x.AnimalSpecies.Code)
                .Select(x => new ServicePackageAnimalSpeciesResponse
                {
                    Id = x.AnimalSpeciesId,
                    Code = x.AnimalSpecies.Code
                })
                .ToList(),

            Employees = package.Employees
                .Where(x =>
                    x.BusinessMembership.IsActive &&
                    x.BusinessMembership.RemovedAt == null &&
                    x.BusinessMembership.User.IsActive &&
                    x.BusinessMembership.Role != BusinessRole.Receptionist)
                .OrderBy(x => x.BusinessMembership.User.FirstName)
                .ThenBy(x => x.BusinessMembership.User.LastName)
                .Select(x => new ServicePackageEmployeeResponse
                {
                    BusinessMembershipId = x.BusinessMembershipId,
                    UserId = x.BusinessMembership.UserId,
                    FirstName = x.BusinessMembership.User.FirstName,
                    LastName = x.BusinessMembership.User.LastName,
                    Role = MapRole(x.BusinessMembership.Role),
                    JobTitle = x.BusinessMembership.JobTitle,
                    ProfileImageUrl = GetPublicUrl(
                        x.BusinessMembership.User.ProfileImageBlobName)
                })
                .ToList(),

            Items = package.Items
                .OrderBy(x => x.SortOrder)
                .Select(x => new ServicePackageItemResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    SortOrder = x.SortOrder,

                    Options = x.Options
                        .OrderBy(o => o.SortOrder)
                        .Select(o => new ServicePackageItemOptionResponse
                        {
                            Id = o.Id,

                            AnimalSpecies =
                                new ServicePackageAnimalSpeciesResponse
                                {
                                    Id = o.AnimalSpeciesId,
                                    Code = o.AnimalSpecies.Code
                                },

                            SortOrder = o.SortOrder,

                            Variants = o.Variants
                                .OrderBy(v => v.SortOrder)
                                .Select(v => new ServicePackageItemVariantResponse
                                {
                                    Id = v.Id,
                                    Name = v.Name,
                                    MinWeightKg = v.MinWeightKg,
                                    MaxWeightKg = v.MaxWeightKg,
                                    Price = v.Price,
                                    IsStartingPrice = v.IsStartingPrice,
                                    IsPriceOnRequest = v.IsPriceOnRequest,
                                    DurationMinutes = v.DurationMinutes,
                                    SortOrder = v.SortOrder
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList(),

            CreatedAt = package.CreatedAt,
            UpdatedAt = package.UpdatedAt
        };
    }

    private string? GetPublicUrl(string? blobName)
    {
        return string.IsNullOrWhiteSpace(blobName)
            ? null
            : _blobStorage.GetPublicUrl(blobName);
    }

    private static BusinessRoleRequest MapRole(BusinessRole role)
    {
        return role switch
        {
            BusinessRole.Owner => BusinessRoleRequest.Owner,
            BusinessRole.Groomer => BusinessRoleRequest.Groomer,
            BusinessRole.Receptionist => BusinessRoleRequest.Receptionist,

            _ => throw new InvalidOperationException(
                "Rolul membrului echipei nu este valid.")
        };
    }
}