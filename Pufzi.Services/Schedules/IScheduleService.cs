using Pufzi.Contracts.Requests.Schedules;
using Pufzi.Contracts.Responses.Schedules;

namespace Pufzi.Services.Schedules;

public interface IScheduleService
{
    Task<BusinessWorkingHoursResponse> GetBusinessWorkingHoursAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<BusinessWorkingHoursResponse> UpdateBusinessWorkingHoursAsync(
        Guid userId,
        Guid businessId,
        UpdateBusinessWorkingHoursRequest request,
        CancellationToken cancellationToken = default);

    Task<EmployeeWorkingHoursResponse> GetEmployeeWorkingHoursAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        CancellationToken cancellationToken = default);

    Task<EmployeeWorkingHoursResponse> UpdateEmployeeWorkingHoursAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        UpdateEmployeeWorkingHoursRequest request,
        CancellationToken cancellationToken = default);
}