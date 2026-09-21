using Pufzi.Contracts.Requests.Businesses;
using Pufzi.Contracts.Responses.Businesses;

namespace Pufzi.Services.Businesses;

public interface IBusinessService
{
    Task<BusinessResponse> CreateAsync(
        Guid userId,
        CreateBusinessRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<BusinessResponse>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<BusinessResponse> GetByIdAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<BusinessResponse> UpdateAsync(
        Guid userId,
        Guid businessId,
        UpdateBusinessRequest request,
        CancellationToken cancellationToken = default);

    Task<BusinessResponse> UploadLogoAsync(
        Guid userId,
        Guid businessId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default);

    Task DeleteLogoAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<BusinessResponse> UploadCoverAsync(
        Guid userId,
        Guid businessId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default);

    Task DeleteCoverAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<BusinessImageResponse> AddImageAsync(
        Guid userId,
        Guid businessId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default);

    Task DeleteImageAsync(
        Guid userId,
        Guid businessId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}