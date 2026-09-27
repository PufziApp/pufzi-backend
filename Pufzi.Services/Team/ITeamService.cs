using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Team;
using Pufzi.Contracts.Responses.Team;

namespace Pufzi.Services.Team;

public interface ITeamService
{
    Task<PagedResponse<TeamMemberResponse>> GetAllAsync(
        Guid userId,
        Guid businessId,
        PaginationRequest request,
        CancellationToken cancellationToken = default);

    Task<TeamMemberDetailsResponse> GetByIdAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        CancellationToken cancellationToken = default);

    Task<TeamMemberDetailsResponse> UpdateAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        UpdateTeamMemberRequest request,
        CancellationToken cancellationToken = default);

    Task<TeamMemberDetailsResponse> UpdateStatusAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        UpdateTeamMemberStatusRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        CancellationToken cancellationToken = default);
}