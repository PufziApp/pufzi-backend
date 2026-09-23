using Pufzi.Contracts.Requests.Team;
using Pufzi.Contracts.Responses.Team;

namespace Pufzi.Services.Team;

public interface ITeamInvitationService
{
    Task<IReadOnlyCollection<BusinessInvitationResponse>> GetAllAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<BusinessInvitationResponse> CreateAsync(
        Guid userId,
        Guid businessId,
        CreateBusinessInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task<BusinessInvitationDetailsResponse> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task AcceptAsync(
        Guid userId,
        AcceptBusinessInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        Guid userId,
        Guid businessId,
        Guid invitationId,
        CancellationToken cancellationToken = default);
}