using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Enums;
using Pufzi.Contracts.Responses.Team;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Infrastructure.Storage;
using Pufzi.Services.Businesses;

namespace Pufzi.Services.Team;

public class TeamService : ITeamService
{
    private readonly PufziDbContext _dbContext;
    private readonly IBusinessAccessService _businessAccess;
    private readonly IBlobStorageService _blobStorage;

    public TeamService(
        PufziDbContext dbContext,
        IBusinessAccessService businessAccess,
        IBlobStorageService blobStorage)
    {
        _dbContext = dbContext;
        _businessAccess = businessAccess;
        _blobStorage = blobStorage;
    }

    public async Task<IReadOnlyCollection<TeamMemberResponse>> GetAllAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        var memberships = await _dbContext.BusinessMemberships
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.BusinessId == businessId)
            .OrderBy(x => x.Role == BusinessRole.Owner ? 0 : 1)
            .ThenBy(x => x.User.FirstName)
            .ThenBy(x => x.User.LastName)
            .ToListAsync(cancellationToken);

        return memberships
            .Select(MapMember)
            .ToList();
    }

    private TeamMemberResponse MapMember(
        BusinessMembership membership)
    {
        return new TeamMemberResponse
        {
            UserId = membership.UserId,
            FirstName = membership.User.FirstName,
            LastName = membership.User.LastName,
            Email = membership.User.Email,
            PhoneNumber = membership.User.PhoneNumber,

            ProfileImageUrl =
                string.IsNullOrWhiteSpace(
                    membership.User.ProfileImageBlobName)
                    ? null
                    : _blobStorage.GetPublicUrl(
                        membership.User.ProfileImageBlobName),

            Role = MapRole(membership.Role),
            JobTitle = membership.JobTitle,
            Bio = membership.Bio,
            IsActive = membership.IsActive,
            JoinedAt = membership.CreatedAt
        };
    }

    private static BusinessRoleRequest MapRole(
        BusinessRole role)
    {
        return role switch
        {
            BusinessRole.Owner =>
                BusinessRoleRequest.Owner,

            BusinessRole.Groomer =>
                BusinessRoleRequest.Groomer,

            BusinessRole.Receptionist =>
                BusinessRoleRequest.Receptionist,

            _ => throw new InvalidOperationException(
                "Rolul membrului echipei nu este valid.")
        };
    }
}