using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Enums;
using Pufzi.Contracts.Requests.Team;
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

    public async Task<PagedResponse<TeamMemberResponse>> GetAllAsync(
    Guid userId,
    Guid businessId,
    PaginationRequest request,
    CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        var query = _dbContext.BusinessMemberships
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x =>
                x.BusinessId == businessId &&
                x.RemovedAt == null);

        var totalCount = await query.CountAsync(
            cancellationToken);

        var memberships = await query
            .OrderBy(x => x.Role == BusinessRole.Owner ? 0 : 1)
            .ThenBy(x => x.User.FirstName)
            .ThenBy(x => x.User.LastName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = memberships
            .Select(MapMember)
            .ToList();

        return new PagedResponse<TeamMemberResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TeamMemberDetailsResponse> GetByIdAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        var membership = await GetTeamMemberAsync(
            businessId,
            teamMemberUserId,
            cancellationToken);

        return MapMemberDetails(membership);
    }

    public async Task<TeamMemberDetailsResponse> UpdateAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        UpdateTeamMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var membership = await GetTeamMemberAsync(
            businessId,
            teamMemberUserId,
            cancellationToken);

        EnsureMemberCanBeManaged(membership);

        var role = MapRole(request.Role);

        if (role == BusinessRole.Owner)
        {
            throw new InvalidOperationException(
                "Rolul de proprietar nu poate fi atribuit unui membru al echipei.");
        }

        membership.Role = role;

        membership.JobTitle =
            NormalizeOptionalText(request.JobTitle);

        membership.Bio =
            NormalizeOptionalText(request.Bio);

        membership.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapMemberDetails(membership);
    }

    public async Task<TeamMemberDetailsResponse> UpdateStatusAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        UpdateTeamMemberStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var membership = await GetTeamMemberAsync(
            businessId,
            teamMemberUserId,
            cancellationToken);

        EnsureMemberCanBeManaged(membership);

        membership.IsActive = request.IsActive;
        membership.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapMemberDetails(membership);
    }

    public async Task RemoveAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var membership = await GetTeamMemberAsync(
            businessId,
            teamMemberUserId,
            cancellationToken);

        EnsureMemberCanBeManaged(membership);

        var now = DateTime.UtcNow;

        membership.IsActive = false;
        membership.RemovedAt = now;
        membership.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<BusinessMembership> GetTeamMemberAsync(
        Guid businessId,
        Guid teamMemberUserId,
        CancellationToken cancellationToken)
    {
        var membership = await _dbContext.BusinessMemberships
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x =>
                    x.BusinessId == businessId &&
                    x.UserId == teamMemberUserId &&
                    x.RemovedAt == null,
                cancellationToken);

        if (membership is null)
        {
            throw new KeyNotFoundException(
                "Membrul echipei nu a fost găsit.");
        }

        return membership;
    }

    private static void EnsureMemberCanBeManaged(
        BusinessMembership membership)
    {
        if (membership.Role == BusinessRole.Owner)
        {
            throw new InvalidOperationException(
                "Proprietarul salonului nu poate fi modificat, dezactivat sau eliminat din echipă.");
        }
    }

    private TeamMemberResponse MapMember(
    BusinessMembership membership)
    {
        return new TeamMemberResponse
        {
            BusinessMembershipId = membership.Id,
            UserId = membership.UserId,
            FirstName = membership.User.FirstName,
            LastName = membership.User.LastName,
            Email = membership.User.Email,
            PhoneNumber = membership.User.PhoneNumber,

            ProfileImageUrl =
                GetPublicUrl(
                    membership.User.ProfileImageBlobName),

            CoverImageUrl =
                GetPublicUrl(
                    membership.CoverImageBlobName),

            Role = MapRole(membership.Role),
            JobTitle = membership.JobTitle,
            Bio = membership.Bio,
            IsActive = membership.IsActive,
            JoinedAt = membership.CreatedAt
        };
    }

    private TeamMemberDetailsResponse MapMemberDetails(
    BusinessMembership membership)
    {
        return new TeamMemberDetailsResponse
        {
            BusinessMembershipId = membership.Id,
            UserId = membership.UserId,
            FirstName = membership.User.FirstName,
            LastName = membership.User.LastName,
            Email = membership.User.Email,
            PhoneNumber = membership.User.PhoneNumber,

            ProfileImageUrl =
                GetPublicUrl(
                    membership.User.ProfileImageBlobName),

            CoverImageUrl =
                GetPublicUrl(
                    membership.CoverImageBlobName),

            Role = MapRole(membership.Role),
            JobTitle = membership.JobTitle,
            Bio = membership.Bio,
            IsActive = membership.IsActive,
            JoinedAt = membership.CreatedAt
        };
    }

    private string? GetPublicUrl(
        string? blobName)
    {
        return string.IsNullOrWhiteSpace(blobName)
            ? null
            : _blobStorage.GetPublicUrl(blobName);
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static BusinessRole MapRole(
        BusinessRoleRequest role)
    {
        return role switch
        {
            BusinessRoleRequest.Groomer =>
                BusinessRole.Groomer,

            BusinessRoleRequest.Receptionist =>
                BusinessRole.Receptionist,

            BusinessRoleRequest.Owner =>
                BusinessRole.Owner,

            _ => throw new InvalidOperationException(
                "Rolul selectat nu este valid.")
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