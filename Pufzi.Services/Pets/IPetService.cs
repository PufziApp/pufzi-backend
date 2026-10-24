using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Pets;
using Pufzi.Contracts.Responses.Pets;

namespace Pufzi.Services.Pets;

public interface IPetService
{
    Task<PagedResponse<PetResponse>> GetMineAsync(
        Guid userId,
        PaginationRequest pagination,
        bool archived,
        CancellationToken cancellationToken = default);

    Task<PetResponse> GetByIdAsync(
        Guid userId,
        Guid petId,
        CancellationToken cancellationToken = default);

    Task<PetResponse> CreateAsync(
        Guid userId,
        SavePetRequest request,
        CancellationToken cancellationToken = default);

    Task<PetResponse> UpdateAsync(
        Guid userId,
        Guid petId,
        SavePetRequest request,
        CancellationToken cancellationToken = default);

    Task ArchiveAsync(
        Guid userId,
        Guid petId,
        CancellationToken cancellationToken = default);

    Task RestoreAsync(
        Guid userId,
        Guid petId,
        CancellationToken cancellationToken = default);

    Task<PetResponse> UploadProfileImageAsync(
        Guid userId,
        Guid petId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default);

    Task DeleteProfileImageAsync(
        Guid userId,
        Guid petId,
        CancellationToken cancellationToken = default);

    Task<PetImageResponse> AddImageAsync(
        Guid userId,
        Guid petId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default);

    Task DeleteImageAsync(
        Guid userId,
        Guid petId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}