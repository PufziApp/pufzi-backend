using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Services;
using Pufzi.Contracts.Responses.Services;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Infrastructure.Storage;
using Pufzi.Services.Businesses;

namespace Pufzi.Services.Services;

public class ServiceService : IServiceService
{
    private readonly PufziDbContext _dbContext;
    private readonly IBusinessAccessService _businessAccess;
    private readonly IBlobStorageService _blobStorage;

    public ServiceService(
        PufziDbContext dbContext,
        IBusinessAccessService businessAccess,
        IBlobStorageService blobStorage)
    {
        _dbContext = dbContext;
        _businessAccess = businessAccess;
        _blobStorage = blobStorage;
    }

    public async Task<PagedResponse<ServiceResponse>> GetAllAsync(
        Guid userId,
        Guid businessId,
        GetServicesRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        var query = _dbContext.Services
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(
                    x.Name,
                    $"%{search}%") ||
                (
                    x.Description != null &&
                    EF.Functions.ILike(
                        x.Description,
                        $"%{search}%")
                ));
        }

        if (request.AnimalSpeciesId.HasValue)
        {
            var speciesId =
                request.AnimalSpeciesId.Value;

            query = query.Where(x =>
                x.Options.Any(o =>
                    o.AnimalSpeciesId == speciesId));
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
                x.Options
                    .SelectMany(o => o.PriceVariants)
                    .Any(v =>
                        v.Price.HasValue &&
                        v.Price.Value >= minPrice));
        }

        if (request.MaxPrice.HasValue)
        {
            var maxPrice = request.MaxPrice.Value;

            query = query.Where(x =>
                x.Options
                    .SelectMany(o => o.PriceVariants)
                    .Any(v =>
                        v.Price.HasValue &&
                        v.Price.Value <= maxPrice));
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        query = ApplySorting(
            query,
            request.SortBy,
            request.SortDirection);

        var services = await query
            .Skip(
                (request.Page - 1) *
                request.PageSize)
            .Take(request.PageSize)
            .Include(x => x.Options)
                .ThenInclude(x => x.AnimalSpecies)
            .Include(x => x.Options)
                .ThenInclude(x => x.PriceVariants)
            .ToListAsync(cancellationToken);

        var items = services
            .Select(MapListItem)
            .ToList();

        return new PagedResponse<ServiceResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServiceDetailsResponse> GetByIdAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        var service =
            await GetServiceAsync(
                businessId,
                serviceId,
                cancellationToken);

        return MapDetails(service);
    }

    public async Task<ServiceDetailsResponse> CreateAsync(
        Guid userId,
        Guid businessId,
        CreateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        await ValidateOptionsAsync(
            request.Options,
            cancellationToken);

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                "Numele serviciului este obligatoriu.");
        }

        var now = DateTime.UtcNow;

        var service = new Service
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = name,
            Description = Clean(request.Description),
            IsActive = true,
            SortOrder = request.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
            Options = request.Options
                .Select(MapOption)
                .ToList()
        };

        _dbContext.Services.Add(service);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var created =
            await GetServiceAsync(
                businessId,
                service.Id,
                cancellationToken);

        return MapDetails(created);
    }

    public async Task<ServiceDetailsResponse> UpdateAsync(
    Guid userId,
    Guid businessId,
    Guid serviceId,
    UpdateServiceRequest request,
    CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        await ValidateOptionsAsync(
            request.Options,
            cancellationToken);

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                "Numele serviciului este obligatoriu.");
        }

        var service =
            await _dbContext.Services
                .Include(x => x.Options)
                    .ThenInclude(x => x.PriceVariants)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == serviceId &&
                        x.BusinessId == businessId,
                    cancellationToken);

        if (service is null)
        {
            throw new KeyNotFoundException(
                "Serviciul nu a fost găsit.");
        }

        service.Name = name;
        service.Description =
            Clean(request.Description);
        service.SortOrder =
            request.SortOrder;
        service.UpdatedAt =
            DateTime.UtcNow;

        var oldOptions =
            service.Options.ToList();

        var oldPriceVariants =
            oldOptions
                .SelectMany(x => x.PriceVariants)
                .ToList();

        if (oldPriceVariants.Count > 0)
        {
            _dbContext.ServicePriceVariants
                .RemoveRange(oldPriceVariants);
        }

        if (oldOptions.Count > 0)
        {
            _dbContext.ServiceOptions
                .RemoveRange(oldOptions);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var newOptions =
            request.Options
                .Select(MapOption)
                .ToList();

        foreach (var option in newOptions)
        {
            option.ServiceId =
                service.Id;
        }

        _dbContext.ServiceOptions
            .AddRange(newOptions);

        service.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var updated =
            await GetServiceAsync(
                businessId,
                serviceId,
                cancellationToken);

        return MapDetails(updated);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var service =
            await _dbContext.Services
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == serviceId &&
                        x.BusinessId == businessId,
                    cancellationToken);

        if (service is null)
        {
            throw new KeyNotFoundException(
                "Serviciul nu a fost găsit.");
        }

        if (!service.IsActive)
        {
            return;
        }

        service.IsActive = false;
        service.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task RestoreAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var service =
            await _dbContext.Services
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == serviceId &&
                        x.BusinessId == businessId,
                    cancellationToken);

        if (service is null)
        {
            throw new KeyNotFoundException(
                "Serviciul nu a fost găsit.");
        }

        if (service.IsActive)
        {
            return;
        }

        service.IsActive = true;
        service.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<ServiceDetailsResponse> UploadImageAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var service =
            await GetServiceAsync(
                businessId,
                serviceId,
                cancellationToken);

        var oldBlobName =
            service.ImageBlobName;

        var blobName =
            $"businesses/{businessId}/services/{serviceId}/{Guid.NewGuid():N}{extension}";

        await _blobStorage.UploadPublicAsync(
            stream,
            blobName,
            contentType,
            cancellationToken);

        service.ImageBlobName = blobName;
        service.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(oldBlobName))
        {
            await _blobStorage.DeletePublicAsync(
                oldBlobName,
                cancellationToken);
        }

        return MapDetails(service);
    }

    public async Task DeleteImageAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var service =
            await _dbContext.Services
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == serviceId &&
                        x.BusinessId == businessId,
                    cancellationToken);

        if (service is null)
        {
            throw new KeyNotFoundException(
                "Serviciul nu a fost găsit.");
        }

        if (string.IsNullOrWhiteSpace(
                service.ImageBlobName))
        {
            return;
        }

        var blobName =
            service.ImageBlobName;

        service.ImageBlobName = null;
        service.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await _blobStorage.DeletePublicAsync(
            blobName,
            cancellationToken);
    }

    private async Task<Service> GetServiceAsync(
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var service =
            await _dbContext.Services
                .Include(x => x.Options)
                    .ThenInclude(x =>
                        x.AnimalSpecies)
                .Include(x => x.Options)
                    .ThenInclude(x =>
                        x.PriceVariants)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == serviceId &&
                        x.BusinessId == businessId,
                    cancellationToken);

        if (service is null)
        {
            throw new KeyNotFoundException(
                "Serviciul nu a fost găsit.");
        }

        return service;
    }

    private async Task ValidateOptionsAsync(
        ICollection<CreateServiceOptionRequest> options,
        CancellationToken cancellationToken)
    {
        if (options.Count == 0)
        {
            throw new InvalidOperationException(
                "Serviciul trebuie să fie disponibil pentru cel puțin o specie.");
        }

        var speciesIds = options
            .Select(x => x.AnimalSpeciesId)
            .ToList();

        if (speciesIds.Any(x => x == Guid.Empty))
        {
            throw new InvalidOperationException(
                "Specia animalului este obligatorie.");
        }

        if (speciesIds.Count !=
            speciesIds.Distinct().Count())
        {
            throw new InvalidOperationException(
                "Aceeași specie nu poate fi adăugată de mai multe ori.");
        }

        var existingSpeciesCount =
            await _dbContext.AnimalSpecies
                .CountAsync(
                    x =>
                        speciesIds.Contains(x.Id) &&
                        x.IsActive,
                    cancellationToken);

        if (existingSpeciesCount !=
            speciesIds.Count)
        {
            throw new InvalidOperationException(
                "Una sau mai multe specii de animale nu sunt valide.");
        }

        foreach (var option in options)
        {
            if (option.PriceVariants.Count == 0)
            {
                throw new InvalidOperationException(
                    "Fiecare specie trebuie să aibă cel puțin o variantă de preț.");
            }

            foreach (var variant in option.PriceVariants)
            {
                if (string.IsNullOrWhiteSpace(
                        variant.Name))
                {
                    throw new InvalidOperationException(
                        "Numele variantei de preț este obligatoriu.");
                }

                if (variant.MinWeightKg.HasValue &&
                    variant.MaxWeightKg.HasValue &&
                    variant.MinWeightKg.Value >
                    variant.MaxWeightKg.Value)
                {
                    throw new InvalidOperationException(
                        "Greutatea minimă nu poate fi mai mare decât greutatea maximă.");
                }

                if (variant.IsPriceOnRequest)
                {
                    if (variant.Price.HasValue)
                    {
                        throw new InvalidOperationException(
                            "O variantă cu preț la cerere nu poate avea și un preț.");
                    }

                    if (variant.IsStartingPrice)
                    {
                        throw new InvalidOperationException(
                            "Prețul la cerere nu poate fi și preț de pornire.");
                    }
                }
                else if (!variant.Price.HasValue)
                {
                    throw new InvalidOperationException(
                        "Varianta trebuie să aibă un preț sau să fie marcată ca preț la cerere.");
                }
            }
        }
    }

    private static ServiceOption MapOption(
        CreateServiceOptionRequest request)
    {
        return new ServiceOption
        {
            Id = Guid.NewGuid(),
            AnimalSpeciesId =
                request.AnimalSpeciesId,
            DurationMinutes =
                request.DurationMinutes,
            SortOrder =
                request.SortOrder,

            PriceVariants = request.PriceVariants
                .Select(x =>
                    new ServicePriceVariant
                    {
                        Id = Guid.NewGuid(),
                        Name = x.Name.Trim(),
                        MinWeightKg =
                            x.MinWeightKg,
                        MaxWeightKg =
                            x.MaxWeightKg,
                        Price =
                            x.Price,
                        IsStartingPrice =
                            x.IsStartingPrice,
                        IsPriceOnRequest =
                            x.IsPriceOnRequest,
                        DurationMinutes =
                            x.DurationMinutes,
                        SortOrder =
                            x.SortOrder
                    })
                .ToList()
        };
    }

    private static IQueryable<Service> ApplySorting(
        IQueryable<Service> query,
        ServiceSortBy sortBy,
        SortDirection direction)
    {
        return (sortBy, direction) switch
        {
            (ServiceSortBy.Name, SortDirection.Asc) =>
                query
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.Id),

            (ServiceSortBy.Name, SortDirection.Desc) =>
                query
                    .OrderByDescending(x => x.Name)
                    .ThenBy(x => x.Id),

            (ServiceSortBy.Price, SortDirection.Asc) =>
                query
                    .OrderBy(x => x.Options
                        .SelectMany(o =>
                            o.PriceVariants)
                        .Where(v =>
                            v.Price.HasValue)
                        .Min(v => v.Price))
                    .ThenBy(x => x.Id),

            (ServiceSortBy.Price, SortDirection.Desc) =>
                query
                    .OrderByDescending(x => x.Options
                        .SelectMany(o =>
                            o.PriceVariants)
                        .Where(v =>
                            v.Price.HasValue)
                        .Min(v => v.Price))
                    .ThenBy(x => x.Id),

            (ServiceSortBy.CreatedAt, SortDirection.Asc) =>
                query
                    .OrderBy(x => x.CreatedAt)
                    .ThenBy(x => x.Id),

            (ServiceSortBy.CreatedAt, SortDirection.Desc) =>
                query
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenBy(x => x.Id),

            (ServiceSortBy.UpdatedAt, SortDirection.Asc) =>
                query
                    .OrderBy(x => x.UpdatedAt)
                    .ThenBy(x => x.Id),

            (ServiceSortBy.UpdatedAt, SortDirection.Desc) =>
                query
                    .OrderByDescending(x => x.UpdatedAt)
                    .ThenBy(x => x.Id),

            (_, SortDirection.Desc) =>
                query
                    .OrderByDescending(x =>
                        x.SortOrder)
                    .ThenBy(x => x.Id),

            _ =>
                query
                    .OrderBy(x =>
                        x.SortOrder)
                    .ThenBy(x => x.Id)
        };
    }

    private ServiceResponse MapListItem(
        Service service)
    {
        var prices = service.Options
            .SelectMany(x => x.PriceVariants)
            .Where(x => x.Price.HasValue)
            .Select(x => x.Price!.Value)
            .ToList();

        return new ServiceResponse
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,

            ImageUrl =
                string.IsNullOrWhiteSpace(
                    service.ImageBlobName)
                    ? null
                    : _blobStorage.GetPublicUrl(
                        service.ImageBlobName),

            StartingPrice =
                prices.Count == 0
                    ? null
                    : prices.Min(),

            HasPriceOnRequest =
                service.Options
                    .SelectMany(x =>
                        x.PriceVariants)
                    .Any(x =>
                        x.IsPriceOnRequest),

            IsActive =
                service.IsActive,

            SortOrder =
                service.SortOrder,

            AnimalSpecies =
                service.Options
                    .OrderBy(x => x.SortOrder)
                    .Select(x =>
                        new AnimalSpeciesResponse
                        {
                            Id =
                                x.AnimalSpecies.Id,
                            Code =
                                x.AnimalSpecies.Code
                        })
                    .ToList(),

            CreatedAt =
                service.CreatedAt,

            UpdatedAt =
                service.UpdatedAt
        };
    }

    private ServiceDetailsResponse MapDetails(
        Service service)
    {
        return new ServiceDetailsResponse
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,

            ImageUrl =
                string.IsNullOrWhiteSpace(
                    service.ImageBlobName)
                    ? null
                    : _blobStorage.GetPublicUrl(
                        service.ImageBlobName),

            IsActive =
                service.IsActive,

            SortOrder =
                service.SortOrder,

            Options =
                service.Options
                    .OrderBy(x => x.SortOrder)
                    .Select(option =>
                        new ServiceOptionResponse
                        {
                            Id = option.Id,

                            AnimalSpecies =
                                new AnimalSpeciesResponse
                                {
                                    Id =
                                        option.AnimalSpecies.Id,
                                    Code =
                                        option.AnimalSpecies.Code
                                },

                            DurationMinutes =
                                option.DurationMinutes,

                            SortOrder =
                                option.SortOrder,

                            PriceVariants =
                                option.PriceVariants
                                    .OrderBy(x =>
                                        x.SortOrder)
                                    .Select(variant =>
                                        new ServicePriceVariantResponse
                                        {
                                            Id =
                                                variant.Id,
                                            Name =
                                                variant.Name,
                                            MinWeightKg =
                                                variant.MinWeightKg,
                                            MaxWeightKg =
                                                variant.MaxWeightKg,
                                            Price =
                                                variant.Price,
                                            IsStartingPrice =
                                                variant.IsStartingPrice,
                                            IsPriceOnRequest =
                                                variant.IsPriceOnRequest,
                                            DurationMinutes =
                                                variant.DurationMinutes,
                                            SortOrder =
                                                variant.SortOrder
                                        })
                                    .ToList()
                        })
                    .ToList(),

            CreatedAt =
                service.CreatedAt,

            UpdatedAt =
                service.UpdatedAt
        };
    }

    private static string? Clean(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}