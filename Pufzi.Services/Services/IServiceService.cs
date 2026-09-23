using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Services;
using Pufzi.Contracts.Responses.Services;

namespace Pufzi.Services.Services;

public interface IServiceService
{
    Task<PagedResponse<ServiceResponse>> GetAllAsync(
        Guid userId,
        Guid businessId,
        GetServicesRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceDetailsResponse> GetByIdAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<ServiceDetailsResponse> CreateAsync(
        Guid userId,
        Guid businessId,
        CreateServiceRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceDetailsResponse> UpdateAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        UpdateServiceRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task RestoreAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<ServiceDetailsResponse> UploadImageAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        Stream stream,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default);

    Task DeleteImageAsync(
        Guid userId,
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken = default);
}