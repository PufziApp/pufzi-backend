using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.ServicePackages;
using Pufzi.Contracts.Responses.ServicePackages;

namespace Pufzi.Services.Salons;

public interface ISalonPackageService
{
    Task<PagedResponse<ServicePackageResponse>> GetAllAsync(
        Guid salonId,
        GetServicePackagesRequest request,
        CancellationToken cancellationToken = default);

    Task<ServicePackageDetailsResponse> GetByIdAsync(
        Guid salonId,
        Guid packageId,
        CancellationToken cancellationToken = default);
}