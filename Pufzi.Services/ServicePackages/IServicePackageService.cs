using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.ServicePackages;
using Pufzi.Contracts.Responses.ServicePackages;

namespace Pufzi.Services.ServicePackages;

public interface IServicePackageService
{
    Task<PagedResponse<ServicePackageResponse>> GetAllAsync(
        Guid userId,
        Guid businessId,
        GetServicePackagesRequest request,
        CancellationToken cancellationToken = default);

    Task<ServicePackageDetailsResponse> GetByIdAsync(
        Guid userId,
        Guid businessId,
        Guid packageId,
        CancellationToken cancellationToken = default);

    Task<ServicePackageDetailsResponse> CreateAsync(
        Guid userId,
        Guid businessId,
        CreateServicePackageRequest request,
        CancellationToken cancellationToken = default);

    Task<ServicePackageDetailsResponse> UpdateAsync(
        Guid userId,
        Guid businessId,
        Guid packageId,
        UpdateServicePackageRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid userId,
        Guid businessId,
        Guid packageId,
        CancellationToken cancellationToken = default);

    Task RestoreAsync(
        Guid userId,
        Guid businessId,
        Guid packageId,
        CancellationToken cancellationToken = default);
}