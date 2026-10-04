using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Requests.Schedules;
using Pufzi.Contracts.Responses.Schedules;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;
using Pufzi.Services.Businesses;

namespace Pufzi.Services.Schedules;

public class ScheduleService : IScheduleService
{
    private readonly PufziDbContext _dbContext;
    private readonly IBusinessAccessService _businessAccess;

    public ScheduleService(
        PufziDbContext dbContext,
        IBusinessAccessService businessAccess)
    {
        _dbContext = dbContext;
        _businessAccess = businessAccess;
    }

    public async Task<BusinessWorkingHoursResponse> GetBusinessWorkingHoursAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        var workingHours = await _dbContext.BusinessWorkingHours
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        return MapBusinessWorkingHours(
            businessId,
            workingHours);
    }

    public async Task<BusinessWorkingHoursResponse> UpdateBusinessWorkingHoursAsync(
        Guid userId,
        Guid businessId,
        UpdateBusinessWorkingHoursRequest request,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        ValidateBusinessSchedule(request);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await _dbContext.BusinessWorkingHours
            .Where(x => x.BusinessId == businessId)
            .ExecuteDeleteAsync(cancellationToken);

        var workingHours = new List<BusinessWorkingHour>();

        foreach (var day in request.Days)
        {
            if (!day.IsOpen)
            {
                workingHours.Add(
                    new BusinessWorkingHour
                    {
                        Id = Guid.NewGuid(),
                        BusinessId = businessId,
                        DayOfWeek = day.DayOfWeek,
                        IsOpen = false,
                        StartTime = null,
                        EndTime = null
                    });

                continue;
            }

            foreach (var interval in day.Intervals)
            {
                workingHours.Add(
                    new BusinessWorkingHour
                    {
                        Id = Guid.NewGuid(),
                        BusinessId = businessId,
                        DayOfWeek = day.DayOfWeek,
                        IsOpen = true,
                        StartTime = interval.StartTime,
                        EndTime = interval.EndTime
                    });
            }
        }

        _dbContext.BusinessWorkingHours.AddRange(
            workingHours);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return MapBusinessWorkingHours(
            businessId,
            workingHours);
    }

    public async Task<EmployeeWorkingHoursResponse> GetEmployeeWorkingHoursAsync(
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

        var workingHours = await _dbContext.EmployeeWorkingHours
            .AsNoTracking()
            .Where(x =>
                x.BusinessMembershipId == membership.Id)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        return MapEmployeeWorkingHours(
            membership,
            workingHours);
    }

    public async Task<EmployeeWorkingHoursResponse> UpdateEmployeeWorkingHoursAsync(
        Guid userId,
        Guid businessId,
        Guid teamMemberUserId,
        UpdateEmployeeWorkingHoursRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentMembership =
            await _businessAccess.RequireMembershipAsync(
                userId,
                businessId,
                cancellationToken);

        var targetMembership = await GetTeamMemberAsync(
            businessId,
            teamMemberUserId,
            cancellationToken);

        var isOwnSchedule =
            currentMembership.UserId == targetMembership.UserId;

        if (!isOwnSchedule &&
            currentMembership.Role != BusinessRole.Owner)
        {
            throw new UnauthorizedAccessException(
                "Poți modifica doar propriul program de lucru.");
        }

        ValidateEmployeeSchedule(request);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await _dbContext.EmployeeWorkingHours
            .Where(x =>
                x.BusinessMembershipId == targetMembership.Id)
            .ExecuteDeleteAsync(cancellationToken);

        var workingHours = new List<EmployeeWorkingHour>();

        foreach (var day in request.Days)
        {
            if (!day.IsWorking)
            {
                workingHours.Add(
                    new EmployeeWorkingHour
                    {
                        Id = Guid.NewGuid(),
                        BusinessMembershipId =
                            targetMembership.Id,
                        DayOfWeek = day.DayOfWeek,
                        IsWorking = false,
                        StartTime = null,
                        EndTime = null
                    });

                continue;
            }

            foreach (var interval in day.Intervals)
            {
                workingHours.Add(
                    new EmployeeWorkingHour
                    {
                        Id = Guid.NewGuid(),
                        BusinessMembershipId =
                            targetMembership.Id,
                        DayOfWeek = day.DayOfWeek,
                        IsWorking = true,
                        StartTime = interval.StartTime,
                        EndTime = interval.EndTime
                    });
            }
        }

        _dbContext.EmployeeWorkingHours.AddRange(
            workingHours);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return MapEmployeeWorkingHours(
            targetMembership,
            workingHours);
    }

    private async Task<BusinessMembership> GetTeamMemberAsync(
        Guid businessId,
        Guid teamMemberUserId,
        CancellationToken cancellationToken)
    {
        var membership =
            await _dbContext.BusinessMemberships
                .AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x =>
                        x.BusinessId == businessId &&
                        x.UserId == teamMemberUserId &&
                        x.IsActive &&
                        x.RemovedAt == null,
                    cancellationToken);

        if (membership is null)
        {
            throw new KeyNotFoundException(
                "Membrul echipei nu a fost găsit sau nu este activ.");
        }

        return membership;
    }

    private static void ValidateBusinessSchedule(
        UpdateBusinessWorkingHoursRequest request)
    {
        EnsureUniqueDays(
            request.Days.Select(x => x.DayOfWeek));

        foreach (var day in request.Days)
        {
            if (!day.IsOpen)
            {
                if (day.Intervals.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"Ziua {day.DayOfWeek} este închisă și nu poate avea intervale orare.");
                }

                continue;
            }

            if (day.Intervals.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Ziua {day.DayOfWeek} este deschisă și trebuie să aibă cel puțin un interval orar.");
            }

            ValidateIntervals(
                day.DayOfWeek,
                day.Intervals);
        }
    }

    private static void ValidateEmployeeSchedule(
        UpdateEmployeeWorkingHoursRequest request)
    {
        EnsureUniqueDays(
            request.Days.Select(x => x.DayOfWeek));

        foreach (var day in request.Days)
        {
            if (!day.IsWorking)
            {
                if (day.Intervals.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"Ziua {day.DayOfWeek} este nelucrătoare și nu poate avea intervale orare.");
                }

                continue;
            }

            if (day.Intervals.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Ziua {day.DayOfWeek} este marcată ca lucrătoare și trebuie să aibă cel puțin un interval orar.");
            }

            ValidateIntervals(
                day.DayOfWeek,
                day.Intervals);
        }
    }

    private static void EnsureUniqueDays(
        IEnumerable<DayOfWeek> days)
    {
        var dayList = days.ToList();

        if (dayList.Count != dayList.Distinct().Count())
        {
            throw new InvalidOperationException(
                "Aceeași zi a săptămânii nu poate fi trimisă de mai multe ori.");
        }
    }

    private static void ValidateIntervals(
        DayOfWeek dayOfWeek,
        IEnumerable<WorkingTimeIntervalRequest> intervals)
    {
        var orderedIntervals = intervals
            .OrderBy(x => x.StartTime)
            .ToList();

        for (var index = 0;
             index < orderedIntervals.Count;
             index++)
        {
            var current = orderedIntervals[index];

            if (current.StartTime >= current.EndTime)
            {
                throw new InvalidOperationException(
                    $"Intervalul pentru {dayOfWeek} trebuie să aibă ora de început înaintea orei de sfârșit.");
            }

            if (index == 0)
            {
                continue;
            }

            var previous =
                orderedIntervals[index - 1];

            if (current.StartTime < previous.EndTime)
            {
                throw new InvalidOperationException(
                    $"Intervalele pentru {dayOfWeek} nu se pot suprapune.");
            }
        }
    }

    private static BusinessWorkingHoursResponse MapBusinessWorkingHours(
        Guid businessId,
        IReadOnlyCollection<BusinessWorkingHour> workingHours)
    {
        var days = Enum
            .GetValues<DayOfWeek>()
            .Select(dayOfWeek =>
            {
                var rows = workingHours
                    .Where(x =>
                        x.DayOfWeek == dayOfWeek)
                    .OrderBy(x => x.StartTime)
                    .ToList();

                var openRows = rows
                    .Where(x =>
                        x.IsOpen &&
                        x.StartTime.HasValue &&
                        x.EndTime.HasValue)
                    .ToList();

                return new BusinessWorkingDayResponse
                {
                    DayOfWeek = dayOfWeek,
                    IsOpen = openRows.Count > 0,

                    Intervals = openRows
                        .Select(x =>
                            new WorkingTimeIntervalResponse
                            {
                                StartTime =
                                    x.StartTime!.Value,
                                EndTime =
                                    x.EndTime!.Value
                            })
                        .ToList()
                };
            })
            .ToList();

        return new BusinessWorkingHoursResponse
        {
            BusinessId = businessId,
            Days = days
        };
    }

    private static EmployeeWorkingHoursResponse MapEmployeeWorkingHours(
        BusinessMembership membership,
        IReadOnlyCollection<EmployeeWorkingHour> workingHours)
    {
        var days = Enum
            .GetValues<DayOfWeek>()
            .Select(dayOfWeek =>
            {
                var rows = workingHours
                    .Where(x =>
                        x.DayOfWeek == dayOfWeek)
                    .OrderBy(x => x.StartTime)
                    .ToList();

                var workingRows = rows
                    .Where(x =>
                        x.IsWorking &&
                        x.StartTime.HasValue &&
                        x.EndTime.HasValue)
                    .ToList();

                return new EmployeeWorkingDayResponse
                {
                    DayOfWeek = dayOfWeek,
                    IsWorking =
                        workingRows.Count > 0,

                    Intervals = workingRows
                        .Select(x =>
                            new WorkingTimeIntervalResponse
                            {
                                StartTime =
                                    x.StartTime!.Value,
                                EndTime =
                                    x.EndTime!.Value
                            })
                        .ToList()
                };
            })
            .ToList();

        return new EmployeeWorkingHoursResponse
        {
            BusinessMembershipId =
                membership.Id,

            UserId =
                membership.UserId,

            FirstName =
                membership.User.FirstName,

            LastName =
                membership.User.LastName,

            Days = days
        };
    }
}