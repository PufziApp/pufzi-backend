using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.Businesses;

public interface IBusinessAccessService
{
    Task<BusinessMembership> RequireMembershipAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<BusinessMembership> RequireOwnerAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<BusinessMembership> RequireRoleAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default,
        params BusinessRole[] allowedRoles);
}