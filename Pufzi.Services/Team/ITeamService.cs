using Pufzi.Contracts.Responses.Team;

namespace Pufzi.Services.Team;

public interface ITeamService
{
    Task<IReadOnlyCollection<TeamMemberResponse>> GetAllAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default);
}