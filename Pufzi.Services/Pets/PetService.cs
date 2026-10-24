
using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Pets;
using Pufzi.Contracts.Responses.Pets;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Infrastructure.Storage;

namespace Pufzi.Services.Pets;

public class PetService : IPetService
{
    private readonly PufziDbContext _dbContext;
    private readonly IBlobStorageService _blobStorage;

    public PetService(
        PufziDbContext dbContext,
        IBlobStorageService blobStorage)
    {
        _dbContext = dbContext;
        _blobStorage = blobStorage;
    }

    public async Task<PagedResponse<PetResponse>> GetMineAsync(
        Guid userId,
        PaginationRequest pagination,
        bool archived,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Pets
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.IsArchived == archived);

        var totalCount = await query.CountAsync(
            cancellationToken);

        var pets = await query
            .Include(x => x.AnimalSpecies)
            .Include(x => x.AnimalBreed)
            .Include(x => x.Images)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return new PagedResponse<PetResponse>
        {
            Items = pets
                .Select(MapToResponse)
                .ToList(),

            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PetResponse> GetByIdAsync(
        Guid userId,
        Guid petId,
        CancellationToken cancellationToken = default)
    {
        var pet = await GetOwnedPetAsync(
            userId,
            petId,
            cancellationToken);

        return MapToResponse(pet);
    }

    public async Task<PetResponse> CreateAsync(
        Guid userId,
        SavePetRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(
            request,
            cancellationToken);

        var now = DateTime.UtcNow;

        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            AnimalSpeciesId = request.AnimalSpeciesId,
            AnimalBreedId = request.AnimalBreedId,
            CreatedAt = now,
            UpdatedAt = now
        };

        ApplyRequest(pet, request);

        _dbContext.Pets.Add(pet);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetByIdAsync(
            userId,
            pet.Id,
            cancellationToken);
    }

    public async Task<PetResponse> UpdateAsync(
        Guid userId,
        Guid petId,
        SavePetRequest request,
        CancellationToken cancellationToken = default)
    {
        var pet = await GetOwnedPetAsync(
            userId,
            petId,
            cancellationToken);

        EnsureNotArchived(pet);

        await ValidateRequestAsync(
            request,
            cancellationToken);

        ApplyRequest(pet, request);

        pet.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetByIdAsync(
            userId,
            petId,
            cancellationToken);
    }

    public async Task ArchiveAsync(
        Guid userId,
        Guid petId,
        CancellationToken cancellationToken = default)
    {
        var pet = await GetOwnedPetAsync(
            userId,
            petId,
            cancellationToken);

        if (pet.IsArchived)
        {
            return;
        }

        var now = DateTime.UtcNow;

        pet.IsArchived = true;
        pet.ArchivedAt = now;
        pet.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task RestoreAsync(
        Guid userId,
        Guid petId,
        CancellationToken cancellationToken = default)
    {
        var pet = await GetOwnedPetAsync(
            userId,
            petId,
            cancellationToken);

        if (!pet.IsArchived)
        {
            return;
        }

        pet.IsArchived = false;
        pet.ArchivedAt = null;
        pet.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<PetResponse> UploadProfileImageAsync(
        Guid userId,
        Guid petId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        var pet = await GetOwnedPetAsync(
            userId,
            petId,
            cancellationToken);

        EnsureNotArchived(pet);

        var oldBlobName = pet.ProfileImageBlobName;

        var blobName =
            $"pets/{petId}/profile/{Guid.NewGuid():N}{extension}";

        await _blobStorage.UploadPublicAsync(
            stream,
            blobName,
            contentType,
            cancellationToken);

        pet.ProfileImageBlobName = blobName;
        pet.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            try
            {
                await _blobStorage.DeletePublicAsync(
                    blobName,
                    CancellationToken.None);
            }
            catch
            {
                // Nu mascăm eroarea inițială de salvare.
            }

            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldBlobName))
        {
            await _blobStorage.DeletePublicAsync(
                oldBlobName,
                cancellationToken);
        }

        return MapToResponse(pet);
    }

    public async Task DeleteProfileImageAsync(
        Guid userId,
        Guid petId,
        CancellationToken cancellationToken = default)
    {
        var pet = await GetOwnedPetAsync(
            userId,
            petId,
            cancellationToken);

        EnsureNotArchived(pet);

        if (string.IsNullOrWhiteSpace(
                pet.ProfileImageBlobName))
        {
            return;
        }

        var blobName = pet.ProfileImageBlobName;

        pet.ProfileImageBlobName = null;
        pet.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await _blobStorage.DeletePublicAsync(
            blobName,
            cancellationToken);
    }

    public async Task<PetImageResponse> AddImageAsync(
        Guid userId,
        Guid petId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        var pet = await GetOwnedPetAsync(
            userId,
            petId,
            cancellationToken);

        EnsureNotArchived(pet);

        var blobName =
            $"pets/{petId}/gallery/{Guid.NewGuid():N}{extension}";

        await _blobStorage.UploadPublicAsync(
            stream,
            blobName,
            contentType,
            cancellationToken);

        try
        {
            var maxSortOrder = await _dbContext
                .Set<PetImage>()
                .Where(x => x.PetId == petId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync(cancellationToken) ?? -1;

            var image = new PetImage
            {
                Id = Guid.NewGuid(),
                PetId = petId,
                BlobName = blobName,
                SortOrder = maxSortOrder + 1,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Set<PetImage>().Add(image);

            pet.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return new PetImageResponse
            {
                Id = image.Id,
                Url = _blobStorage.GetPublicUrl(
                    image.BlobName),
                SortOrder = image.SortOrder,
                CreatedAt = image.CreatedAt
            };
        }
        catch
        {
            try
            {
                await _blobStorage.DeletePublicAsync(
                    blobName,
                    CancellationToken.None);
            }
            catch
            {
                // Nu mascăm eroarea inițială.
            }

            throw;
        }
    }

    public async Task DeleteImageAsync(
        Guid userId,
        Guid petId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var pet = await GetOwnedPetAsync(
            userId,
            petId,
            cancellationToken);

        EnsureNotArchived(pet);

        var image = await _dbContext.Set<PetImage>()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == imageId &&
                    x.PetId == petId,
                cancellationToken);

        if (image is null)
        {
            throw new KeyNotFoundException(
                "Imaginea nu a fost găsită.");
        }

        var blobName = image.BlobName;

        _dbContext.Set<PetImage>().Remove(image);

        pet.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await _blobStorage.DeletePublicAsync(
            blobName,
            cancellationToken);
    }

    private static void EnsureNotArchived(Pet pet)
    {
        if (pet.IsArchived)
        {
            throw new InvalidOperationException(
                "Reactivează animalul înainte de a modifica informațiile sau fotografiile.");
        }
    }

    private async Task<Pet> GetOwnedPetAsync(
        Guid userId,
        Guid petId,
        CancellationToken cancellationToken)
    {
        var pet = await _dbContext.Pets
            .Include(x => x.AnimalSpecies)
            .Include(x => x.AnimalBreed)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == petId &&
                    x.UserId == userId,
                cancellationToken);

        return pet ?? throw new KeyNotFoundException(
            "Pet not found.");
    }

    private async Task ValidateRequestAsync(
        SavePetRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            request.Name.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Pet name must contain between 1 and 100 characters.");
        }

        if (request.AnimalSpeciesId == Guid.Empty)
        {
            throw new ArgumentException(
                "Animal species is required.");
        }

        var speciesExists = await _dbContext.AnimalSpecies
            .AnyAsync(
                x =>
                    x.Id == request.AnimalSpeciesId &&
                    x.IsActive,
                cancellationToken);

        if (!speciesExists)
        {
            throw new ArgumentException(
                "Invalid animal species.");
        }

        if (request.AnimalBreedId.HasValue)
        {
            var breedExists = await _dbContext.AnimalBreeds
                .AnyAsync(
                    x =>
                        x.Id == request.AnimalBreedId.Value &&
                        x.AnimalSpeciesId == request.AnimalSpeciesId &&
                        x.IsActive,
                    cancellationToken);

            if (!breedExists)
            {
                throw new ArgumentException(
                    "The selected breed does not belong to the species.");
            }
        }

        if (request.BirthDate.HasValue &&
            request.BirthDate.Value >
            DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException(
                "Birth date cannot be in the future.");
        }

        if (request.WeightKg is <= 0 or > 99999 ||
            request.HeightCm is <= 0 or > 99999)
        {
            throw new ArgumentException(
                "Weight and height must be positive.");
        }

        var microchipStatus = ParseEnum(
            request.MicrochipStatus,
            PetAnswer.Unknown);

        var importantStatus = ParseEnum(
            request.ImportantInformationStatus,
            PetAnswer.Unknown);

        ParseEnum(
            request.Sex,
            PetSex.Unknown);

        ParseEnum(
            request.SterilizationStatus,
            PetAnswer.Unknown);

        if (microchipStatus != PetAnswer.Yes &&
            !string.IsNullOrWhiteSpace(
                request.MicrochipNumber))
        {
            throw new ArgumentException(
                "Microchip number requires MicrochipStatus = Yes.");
        }

        if (importantStatus != PetAnswer.Yes &&
            !string.IsNullOrWhiteSpace(
                request.ImportantInformation))
        {
            throw new ArgumentException(
                "Important information requires ImportantInformationStatus = Yes.");
        }

        if (request.Color?.Trim().Length > 100 ||
            request.MicrochipNumber?.Trim().Length > 30 ||
            request.ImportantInformation?.Trim().Length > 2000)
        {
            throw new ArgumentException(
                "One or more text fields exceed the allowed length.");
        }
    }

    private static void ApplyRequest(
        Pet pet,
        SavePetRequest request)
    {
        pet.Name = request.Name.Trim();

        pet.AnimalSpeciesId = request.AnimalSpeciesId;
        pet.AnimalBreedId = request.AnimalBreedId;

        pet.Sex = string.IsNullOrWhiteSpace(request.Sex)
            ? null
            : ParseEnum(
                request.Sex,
                PetSex.Unknown);

        pet.BirthDate = request.BirthDate;
        pet.WeightKg = request.WeightKg;
        pet.HeightCm = request.HeightCm;
        pet.Color = Clean(request.Color);

        pet.MicrochipStatus = ParseEnum(
            request.MicrochipStatus,
            PetAnswer.Unknown);

        pet.MicrochipNumber =
            pet.MicrochipStatus == PetAnswer.Yes
                ? Clean(request.MicrochipNumber)
                : null;

        pet.SterilizationStatus = ParseEnum(
            request.SterilizationStatus,
            PetAnswer.Unknown);

        pet.ImportantInformationStatus = ParseEnum(
            request.ImportantInformationStatus,
            PetAnswer.Unknown);

        pet.ImportantInformation =
            pet.ImportantInformationStatus == PetAnswer.Yes
                ? Clean(request.ImportantInformation)
                : null;
    }

    private static T ParseEnum<T>(
        string? value,
        T defaultValue)
        where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (Enum.TryParse<T>(
                value.Trim(),
                true,
                out var parsed) &&
            Enum.IsDefined(typeof(T), parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            $"Invalid value '{value}' for {typeof(T).Name}.");
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private PetResponse MapToResponse(Pet pet)
    {
        return new PetResponse
        {
            Id = pet.Id,
            UserId = pet.UserId,
            Name = pet.Name,

            AnimalSpeciesId = pet.AnimalSpeciesId,
            AnimalSpeciesCode = pet.AnimalSpecies.Code,

            AnimalBreedId = pet.AnimalBreedId,
            AnimalBreedCode = pet.AnimalBreed?.Code,

            Sex = pet.Sex?.ToString(),
            BirthDate = pet.BirthDate,
            WeightKg = pet.WeightKg,
            HeightCm = pet.HeightCm,
            Color = pet.Color,

            MicrochipStatus =
                pet.MicrochipStatus.ToString(),

            MicrochipNumber = pet.MicrochipNumber,

            SterilizationStatus =
                pet.SterilizationStatus.ToString(),

            ImportantInformationStatus =
                pet.ImportantInformationStatus.ToString(),

            ImportantInformation =
                pet.ImportantInformation,

            HasProfileImage =
                !string.IsNullOrWhiteSpace(
                    pet.ProfileImageBlobName),

            ProfileImageUrl =
                string.IsNullOrWhiteSpace(
                    pet.ProfileImageBlobName)
                    ? null
                    : _blobStorage.GetPublicUrl(
                        pet.ProfileImageBlobName),

            IsArchived = pet.IsArchived,
            ArchivedAt = pet.ArchivedAt,

            CreatedAt = pet.CreatedAt,
            UpdatedAt = pet.UpdatedAt,

            Images = pet.Images
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CreatedAt)
                .Select(x => new PetImageResponse
                {
                    Id = x.Id,

                    Url = _blobStorage.GetPublicUrl(
                        x.BlobName),

                    SortOrder = x.SortOrder,
                    CreatedAt = x.CreatedAt
                })
                .ToList()
        };
    }
}
