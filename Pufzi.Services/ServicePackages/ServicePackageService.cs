using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Enums;
using Pufzi.Contracts.Requests.ServicePackages;
using Pufzi.Contracts.Responses.ServicePackages;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Infrastructure.Storage;
using Pufzi.Services.Businesses;

namespace Pufzi.Services.ServicePackages;

public class ServicePackageService : IServicePackageService
{
    private readonly PufziDbContext _dbContext;
    private readonly IBusinessAccessService _businessAccess;
    private readonly IBlobStorageService _blobStorage;

    public ServicePackageService(
        PufziDbContext dbContext,
        IBusinessAccessService businessAccess,
        IBlobStorageService blobStorage)
    {
        _dbContext = dbContext;
        _businessAccess = businessAccess;
        _blobStorage = blobStorage;
    }

    public async Task<PagedResponse<ServicePackageResponse>> GetAllAsync(
        Guid userId,
        Guid businessId,
        GetServicePackagesRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        ValidatePriceRange(
            request.MinPrice,
            request.MaxPrice);

        var query = _dbContext.ServicePackages
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

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
            var animalSpeciesId =
                request.AnimalSpeciesId.Value;

            query = query.Where(x =>
                x.AnimalSpecies.Any(s =>
                    s.AnimalSpeciesId == animalSpeciesId));
        }

        if (request.EmployeeMembershipId.HasValue)
        {
            var membershipId =
                request.EmployeeMembershipId.Value;

            query = query.Where(x =>
                x.Employees.Any(e =>
                    e.BusinessMembershipId == membershipId));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == request.IsActive.Value);
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

        var totalCount =
            await query.CountAsync(cancellationToken);

        query = ApplySorting(
            query,
            request.SortBy,
            request.SortDirection);

        var packages = await query
            .Include(x => x.AnimalSpecies)
                .ThenInclude(x => x.AnimalSpecies)
            .Include(x => x.Items)
                .ThenInclude(x => x.Options)
                    .ThenInclude(x => x.Variants)
            .Include(x => x.Employees)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<ServicePackageResponse>
        {
            Items = packages
                .Select(MapSummary)
                .ToList(),

            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServicePackageDetailsResponse> GetByIdAsync(
        Guid userId,
        Guid businessId,
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        var package = await GetPackageAsync(
            businessId,
            packageId,
            cancellationToken);

        return MapDetails(package);
    }

    public async Task<ServicePackageDetailsResponse> CreateAsync(
        Guid userId,
        Guid businessId,
        CreateServicePackageRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        ValidatePackageRequest(
            request.AnimalSpeciesIds,
            request.EmployeeMembershipIds,
            request.Items);

        await ValidateAnimalSpeciesAsync(
            request.AnimalSpeciesIds,
            cancellationToken);

        await ValidateEmployeesAsync(
            businessId,
            request.EmployeeMembershipIds,
            cancellationToken);

        ValidateItemSpecies(
            request.AnimalSpeciesIds,
            request.Items);

        var now = DateTime.UtcNow;

        var package = new ServicePackage
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = NormalizeRequiredText(
                request.Name,
                "Numele pachetului este obligatoriu."),
            Description =
                NormalizeOptionalText(request.Description),
            Icon =
                NormalizeOptionalText(request.Icon),
            IsActive = true,
            SortOrder = request.SortOrder,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var animalSpeciesId in
                 request.AnimalSpeciesIds.Distinct())
        {
            package.AnimalSpecies.Add(
                new ServicePackageAnimalSpecies
                {
                    ServicePackageId = package.Id,
                    AnimalSpeciesId = animalSpeciesId
                });
        }

        foreach (var membershipId in
                 request.EmployeeMembershipIds.Distinct())
        {
            package.Employees.Add(
                new ServicePackageEmployee
                {
                    ServicePackageId = package.Id,
                    BusinessMembershipId = membershipId
                });
        }

        foreach (var itemRequest in request.Items)
        {
            package.Items.Add(
                CreateItem(
                    package.Id,
                    itemRequest));
        }

        _dbContext.ServicePackages.Add(package);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var createdPackage = await GetPackageAsync(
            businessId,
            package.Id,
            cancellationToken);

        return MapDetails(createdPackage);
    }

    public async Task<ServicePackageDetailsResponse> UpdateAsync(
        Guid userId,
        Guid businessId,
        Guid packageId,
        UpdateServicePackageRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        ValidatePackageRequest(
            request.AnimalSpeciesIds,
            request.EmployeeMembershipIds,
            request.Items);

        await ValidateAnimalSpeciesAsync(
            request.AnimalSpeciesIds,
            cancellationToken);

        await ValidateEmployeesAsync(
            businessId,
            request.EmployeeMembershipIds,
            cancellationToken);

        ValidateItemSpecies(
            request.AnimalSpeciesIds,
            request.Items);

        var package = await GetPackageAsync(
            businessId,
            packageId,
            cancellationToken);

        package.Name = NormalizeRequiredText(
            request.Name,
            "Numele pachetului este obligatoriu.");

        package.Description =
            NormalizeOptionalText(request.Description);

        package.Icon =
            NormalizeOptionalText(request.Icon);

        package.SortOrder = request.SortOrder;

        package.UpdatedAt = DateTime.UtcNow;

        _dbContext.ServicePackageAnimalSpecies.RemoveRange(
            package.AnimalSpecies);

        _dbContext.ServicePackageEmployees.RemoveRange(
            package.Employees);

        _dbContext.ServicePackageItems.RemoveRange(
            package.Items);

        package.AnimalSpecies.Clear();
        package.Employees.Clear();
        package.Items.Clear();

        foreach (var animalSpeciesId in
                 request.AnimalSpeciesIds.Distinct())
        {
            package.AnimalSpecies.Add(
                new ServicePackageAnimalSpecies
                {
                    ServicePackageId = package.Id,
                    AnimalSpeciesId = animalSpeciesId
                });
        }

        foreach (var membershipId in
                 request.EmployeeMembershipIds.Distinct())
        {
            package.Employees.Add(
                new ServicePackageEmployee
                {
                    ServicePackageId = package.Id,
                    BusinessMembershipId = membershipId
                });
        }

        foreach (var itemRequest in request.Items)
        {
            package.Items.Add(
                CreateItem(
                    package.Id,
                    itemRequest));
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var updatedPackage = await GetPackageAsync(
            businessId,
            package.Id,
            cancellationToken);

        return MapDetails(updatedPackage);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid businessId,
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var package = await _dbContext.ServicePackages
            .FirstOrDefaultAsync(
                x =>
                    x.Id == packageId &&
                    x.BusinessId == businessId,
                cancellationToken);

        if (package is null)
        {
            throw new KeyNotFoundException(
                "Pachetul de servicii nu a fost găsit.");
        }

        if (!package.IsActive)
        {
            return;
        }

        package.IsActive = false;
        package.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task RestoreAsync(
        Guid userId,
        Guid businessId,
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var package = await _dbContext.ServicePackages
            .FirstOrDefaultAsync(
                x =>
                    x.Id == packageId &&
                    x.BusinessId == businessId,
                cancellationToken);

        if (package is null)
        {
            throw new KeyNotFoundException(
                "Pachetul de servicii nu a fost găsit.");
        }

        if (package.IsActive)
        {
            return;
        }

        package.IsActive = true;
        package.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<ServicePackage> GetPackageAsync(
        Guid businessId,
        Guid packageId,
        CancellationToken cancellationToken)
    {
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
                    x.BusinessId == businessId,
                cancellationToken);

        if (package is null)
        {
            throw new KeyNotFoundException(
                "Pachetul de servicii nu a fost găsit.");
        }

        return package;
    }

    private async Task ValidateAnimalSpeciesAsync(
        IReadOnlyCollection<Guid> animalSpeciesIds,
        CancellationToken cancellationToken)
    {
        var ids = animalSpeciesIds
            .Distinct()
            .ToList();

        var existingCount =
            await _dbContext.AnimalSpecies
                .CountAsync(
                    x => ids.Contains(x.Id),
                    cancellationToken);

        if (existingCount != ids.Count)
        {
            throw new InvalidOperationException(
                "Una sau mai multe specii de animale selectate nu sunt valide.");
        }
    }

    private async Task ValidateEmployeesAsync(
        Guid businessId,
        IReadOnlyCollection<Guid> membershipIds,
        CancellationToken cancellationToken)
    {
        var ids = membershipIds
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return;
        }

        var memberships =
            await _dbContext.BusinessMemberships
                .AsNoTracking()
                .Where(x =>
                    ids.Contains(x.Id) &&
                    x.BusinessId == businessId &&
                    x.IsActive &&
                    x.RemovedAt == null)
                .ToListAsync(cancellationToken);

        if (memberships.Count != ids.Count)
        {
            throw new InvalidOperationException(
                "Unul sau mai mulți membri selectați nu sunt membri activi ai salonului.");
        }

        if (memberships.Any(x =>
                x.Role == BusinessRole.Receptionist))
        {
            throw new InvalidOperationException(
                "Un receptionist nu poate fi asignat pentru prestarea unui pachet de servicii.");
        }
    }

    private static void ValidatePackageRequest(
        IReadOnlyCollection<Guid> animalSpeciesIds,
        IReadOnlyCollection<Guid> employeeMembershipIds,
        IReadOnlyCollection<ServicePackageItemRequest> items)
    {
        if (animalSpeciesIds.Count == 0)
        {
            throw new InvalidOperationException(
                "Pachetul trebuie să fie disponibil pentru cel puțin o specie de animal.");
        }

        if (animalSpeciesIds.Any(x => x == Guid.Empty))
        {
            throw new InvalidOperationException(
                "Speciile selectate nu sunt valide.");
        }

        if (animalSpeciesIds.Count !=
            animalSpeciesIds.Distinct().Count())
        {
            throw new InvalidOperationException(
                "Aceeași specie nu poate fi selectată de mai multe ori.");
        }

        if (employeeMembershipIds.Any(x => x == Guid.Empty))
        {
            throw new InvalidOperationException(
                "Membrii echipei selectați nu sunt valizi.");
        }

        if (employeeMembershipIds.Count !=
            employeeMembershipIds.Distinct().Count())
        {
            throw new InvalidOperationException(
                "Același membru al echipei nu poate fi asignat de mai multe ori.");
        }

        if (items.Count == 0)
        {
            throw new InvalidOperationException(
                "Pachetul trebuie să conțină cel puțin un serviciu.");
        }

        foreach (var item in items)
        {
            ValidateItem(item);
        }
    }

    private static void ValidateItem(
        ServicePackageItemRequest item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            throw new InvalidOperationException(
                "Numele serviciului din pachet este obligatoriu.");
        }

        if (item.Options.Count == 0)
        {
            throw new InvalidOperationException(
                $"Serviciul '{item.Name}' trebuie să aibă cel puțin o configurare pentru o specie.");
        }

        var speciesIds = item.Options
            .Select(x => x.AnimalSpeciesId)
            .ToList();

        if (speciesIds.Any(x => x == Guid.Empty))
        {
            throw new InvalidOperationException(
                $"Serviciul '{item.Name}' conține o specie invalidă.");
        }

        if (speciesIds.Count !=
            speciesIds.Distinct().Count())
        {
            throw new InvalidOperationException(
                $"Serviciul '{item.Name}' conține aceeași specie de mai multe ori.");
        }

        foreach (var option in item.Options)
        {
            if (option.Variants.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Serviciul '{item.Name}' trebuie să aibă cel puțin o variantă pentru fiecare specie configurată.");
            }

            foreach (var variant in option.Variants)
            {
                ValidateVariant(
                    item.Name,
                    variant);
            }
        }
    }

    private static void ValidateVariant(
        string itemName,
        ServicePackageItemVariantRequest variant)
    {
        if (string.IsNullOrWhiteSpace(variant.Name))
        {
            throw new InvalidOperationException(
                $"Serviciul '{itemName}' conține o variantă fără nume.");
        }

        if (variant.MinWeightKg.HasValue &&
            variant.MaxWeightKg.HasValue &&
            variant.MinWeightKg.Value >
            variant.MaxWeightKg.Value)
        {
            throw new InvalidOperationException(
                $"Greutatea minimă nu poate fi mai mare decât greutatea maximă pentru varianta '{variant.Name}'.");
        }

        if (variant.IsPriceOnRequest &&
            variant.Price.HasValue)
        {
            throw new InvalidOperationException(
                $"Varianta '{variant.Name}' nu poate avea simultan un preț numeric și preț la cerere.");
        }

        if (!variant.IsPriceOnRequest &&
            !variant.Price.HasValue)
        {
            throw new InvalidOperationException(
                $"Varianta '{variant.Name}' trebuie să aibă un preț sau să fie marcată ca preț la cerere.");
        }

        if (variant.IsPriceOnRequest &&
            variant.IsStartingPrice)
        {
            throw new InvalidOperationException(
                $"Varianta '{variant.Name}' nu poate fi simultan preț de pornire și preț la cerere.");
        }
    }

    private static void ValidateItemSpecies(
        IReadOnlyCollection<Guid> packageSpeciesIds,
        IReadOnlyCollection<ServicePackageItemRequest> items)
    {
        var allowedSpecies =
            packageSpeciesIds.ToHashSet();

        foreach (var item in items)
        {
            foreach (var option in item.Options)
            {
                if (!allowedSpecies.Contains(
                        option.AnimalSpeciesId))
                {
                    throw new InvalidOperationException(
                        $"Serviciul '{item.Name}' folosește o specie care nu este selectată pentru pachet.");
                }
            }
        }
    }

    private static void ValidatePriceRange(
        decimal? minPrice,
        decimal? maxPrice)
    {
        if (minPrice.HasValue &&
            maxPrice.HasValue &&
            minPrice.Value > maxPrice.Value)
        {
            throw new InvalidOperationException(
                "Prețul minim nu poate fi mai mare decât prețul maxim.");
        }
    }

    private static ServicePackageItem CreateItem(
        Guid packageId,
        ServicePackageItemRequest request)
    {
        var item = new ServicePackageItem
        {
            Id = Guid.NewGuid(),
            ServicePackageId = packageId,
            Name = NormalizeRequiredText(
                request.Name,
                "Numele serviciului este obligatoriu."),
            SortOrder = request.SortOrder
        };

        foreach (var optionRequest in request.Options)
        {
            var option = new ServicePackageItemOption
            {
                Id = Guid.NewGuid(),
                ServicePackageItemId = item.Id,
                AnimalSpeciesId =
                    optionRequest.AnimalSpeciesId,
                SortOrder =
                    optionRequest.SortOrder
            };

            foreach (var variantRequest in
                     optionRequest.Variants)
            {
                option.Variants.Add(
                    new ServicePackageItemVariant
                    {
                        Id = Guid.NewGuid(),
                        ServicePackageItemOptionId =
                            option.Id,

                        Name = NormalizeRequiredText(
                            variantRequest.Name,
                            "Numele variantei este obligatoriu."),

                        MinWeightKg =
                            variantRequest.MinWeightKg,

                        MaxWeightKg =
                            variantRequest.MaxWeightKg,

                        Price =
                            variantRequest.Price,

                        IsStartingPrice =
                            variantRequest.IsStartingPrice,

                        IsPriceOnRequest =
                            variantRequest.IsPriceOnRequest,

                        DurationMinutes =
                            variantRequest.DurationMinutes,

                        SortOrder =
                            variantRequest.SortOrder
                    });
            }

            item.Options.Add(option);
        }

        return item;
    }

    private static IQueryable<ServicePackage> ApplySorting(
        IQueryable<ServicePackage> query,
        ServicePackageSortBy sortBy,
        SortDirection direction)
    {
        return (sortBy, direction) switch
        {
            (ServicePackageSortBy.Name, SortDirection.Asc) =>
                query.OrderBy(x => x.Name),

            (ServicePackageSortBy.Name, SortDirection.Desc) =>
                query.OrderByDescending(x => x.Name),

            (ServicePackageSortBy.Price, SortDirection.Asc) =>
                query.OrderBy(x =>
                    x.Items
                        .SelectMany(i => i.Options)
                        .SelectMany(o => o.Variants)
                        .Where(v => v.Price.HasValue)
                        .Select(v => v.Price)
                        .Min()),

            (ServicePackageSortBy.Price, SortDirection.Desc) =>
                query.OrderByDescending(x =>
                    x.Items
                        .SelectMany(i => i.Options)
                        .SelectMany(o => o.Variants)
                        .Where(v => v.Price.HasValue)
                        .Select(v => v.Price)
                        .Min()),

            (ServicePackageSortBy.CreatedAt, SortDirection.Asc) =>
                query.OrderBy(x => x.CreatedAt),

            (ServicePackageSortBy.CreatedAt, SortDirection.Desc) =>
                query.OrderByDescending(x => x.CreatedAt),

            (ServicePackageSortBy.SortOrder, SortDirection.Desc) =>
                query
                    .OrderByDescending(x => x.SortOrder)
                    .ThenBy(x => x.Name),

            _ =>
                query
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
        };
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
                .Min(),

            HasPriceOnRequest =
                variants.Any(x => x.IsPriceOnRequest),

            IsActive = package.IsActive,
            SortOrder = package.SortOrder,

            AnimalSpecies = package.AnimalSpecies
                .OrderBy(x => x.AnimalSpecies.Code)
                .Select(x =>
                    new ServicePackageAnimalSpeciesResponse
                    {
                        Id = x.AnimalSpeciesId,
                        Code = x.AnimalSpecies.Code
                    })
                .ToList(),

            ItemsCount = package.Items.Count,

            EmployeesCount = package.Employees.Count,

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
                .Select(x =>
                    new ServicePackageAnimalSpeciesResponse
                    {
                        Id = x.AnimalSpeciesId,
                        Code = x.AnimalSpecies.Code
                    })
                .ToList(),

            Employees = package.Employees
                .OrderBy(x =>
                    x.BusinessMembership.User.FirstName)
                .ThenBy(x =>
                    x.BusinessMembership.User.LastName)
                .Select(x =>
                    new ServicePackageEmployeeResponse
                    {
                        BusinessMembershipId =
                            x.BusinessMembershipId,

                        UserId =
                            x.BusinessMembership.UserId,

                        FirstName =
                            x.BusinessMembership.User.FirstName,

                        LastName =
                            x.BusinessMembership.User.LastName,

                        Role =
                            MapRole(
                                x.BusinessMembership.Role),

                        JobTitle =
                            x.BusinessMembership.JobTitle,

                        ProfileImageUrl =
                            GetPublicUrl(
                                x.BusinessMembership.User
                                    .ProfileImageBlobName)
                    })
                .ToList(),

            Items = package.Items
                .OrderBy(x => x.SortOrder)
                .Select(x =>
                    new ServicePackageItemResponse
                    {
                        Id = x.Id,
                        Name = x.Name,
                        SortOrder = x.SortOrder,

                        Options = x.Options
                            .OrderBy(o => o.SortOrder)
                            .Select(o =>
                                new ServicePackageItemOptionResponse
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
                                        .Select(v =>
                                            new ServicePackageItemVariantResponse
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

    private string? GetPublicUrl(
        string? blobName)
    {
        return string.IsNullOrWhiteSpace(blobName)
            ? null
            : _blobStorage.GetPublicUrl(blobName);
    }

    private static string NormalizeRequiredText(
        string value,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static BusinessRoleRequest MapRole(
        BusinessRole role)
    {
        return role switch
        {
            BusinessRole.Owner =>
                BusinessRoleRequest.Owner,

            BusinessRole.Groomer =>
                BusinessRoleRequest.Groomer,

            BusinessRole.Receptionist =>
                BusinessRoleRequest.Receptionist,

            _ => throw new InvalidOperationException(
                "Rolul membrului echipei nu este valid.")
        };
    }
}