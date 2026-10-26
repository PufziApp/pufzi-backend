using Pufzi.Contracts.Requests.Salons;
using Pufzi.Contracts.Responses.Salons;

namespace Pufzi.Services.Salons;

public interface ISalonDiscoveryService
{
    Task<SalonListResponse> GetSalonsAsync(
        SalonSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<SalonDetailResponse> GetSalonByIdAsync(
        Guid salonId,
        CancellationToken cancellationToken = default);
}