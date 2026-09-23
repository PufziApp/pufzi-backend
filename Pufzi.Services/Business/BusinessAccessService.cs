using Microsoft.EntityFrameworkCore;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.Businesses;

public class BusinessAccessService : IBusinessAccessService
{
    private readonly PufziDbContext _dbContext;

    public BusinessAccessService(
        PufziDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BusinessMembership> RequireMembershipAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await _dbContext.BusinessMemberships
                .Include(x => x.Business)
                .FirstOrDefaultAsync(
                    x =>
                        x.UserId == userId &&
                        x.BusinessId == businessId &&
                        x.IsActive,
                    cancellationToken);

        if (membership is null)
        {
            throw new UnauthorizedAccessException(
                "Nu ai acces la acest salon.");
        }

        return membership;
    }

    public async Task<BusinessMembership> RequireOwnerAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await RequireMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        if (membership.Role != BusinessRole.Owner)
        {
            throw new UnauthorizedAccessException(
                "Doar proprietarul salonului poate efectua această acțiune.");
        }

        return membership;
    }

    public async Task<BusinessMembership> RequireRoleAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default,
        params BusinessRole[] allowedRoles)
    {
        var membership =
            await RequireMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        if (allowedRoles.Length == 0 ||
            !allowedRoles.Contains(membership.Role))
        {
            throw new UnauthorizedAccessException(
                "Nu ai permisiunea necesară pentru această acțiune.");
        }

        return membership;
    }
}