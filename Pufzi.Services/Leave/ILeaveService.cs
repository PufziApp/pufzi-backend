using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.Leave;
using Pufzi.Contracts.Responses.Leave;

namespace Pufzi.Services.Leave;

public interface ILeaveService
{
    Task<EmployeeLeaveBalanceResponse> GetMyBalanceAsync(
        Guid userId,
        Guid businessId,
        int? year = null,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<LeaveRequestResponse>> GetMyRequestsAsync(
        Guid userId,
        Guid businessId,
        LeaveRequestsFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestResponse> CreateRequestAsync(
        Guid userId,
        Guid businessId,
        CreateLeaveRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestResponse> CancelRequestAsync(
        Guid userId,
        Guid businessId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<LeaveRequestResponse>> GetTeamRequestsAsync(
        Guid userId,
        Guid businessId,
        LeaveRequestsFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<EmployeeLeaveBalanceResponse> GetTeamMemberBalanceAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        int? year = null,
        CancellationToken cancellationToken = default);

    Task<EmployeeLeaveBalanceResponse> UpdateTeamMemberBalanceAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        UpdateEmployeeLeaveBalanceRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestResponse> ApproveRequestAsync(
        Guid userId,
        Guid businessId,
        Guid requestId,
        ReviewLeaveRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestResponse> RejectRequestAsync(
        Guid userId,
        Guid businessId,
        Guid requestId,
        ReviewLeaveRequest request,
        CancellationToken cancellationToken = default);
}