using Pufzi.Contracts.Requests.PublicHolidays;
using Pufzi.Contracts.Responses.PublicHolidays;

namespace Pufzi.Infrastructure.PublicHolidays;

public interface IPublicHolidayService
{
    Task<IReadOnlyCollection<PublicHolidayResponse>> GetPublicHolidaysAsync(
        PublicHolidaysFilterRequest request,
        CancellationToken cancellationToken = default);
}