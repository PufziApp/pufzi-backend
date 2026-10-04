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

    public async Task<IReadOnlyCollection<BusinessScheduleExceptionResponse>>
        GetBusinessScheduleExceptionsAsync(
            Guid userId,
            Guid businessId,
            BusinessScheduleExceptionFilterRequest request,
            CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        ValidateExceptionFilter(request);

        var query = _dbContext.BusinessScheduleExceptions
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        if (request.FromDate.HasValue)
        {
            query = query.Where(x =>
                x.Date >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x =>
                x.Date <= request.ToDate.Value);
        }

        var exceptions = await query
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return exceptions
            .Select(MapBusinessScheduleException)
            .ToList();
    }

    public async Task<BusinessScheduleExceptionResponse>
        CreateBusinessScheduleExceptionAsync(
            Guid userId,
            Guid businessId,
            CreateBusinessScheduleExceptionRequest request,
            CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        ValidateScheduleException(
            request.IsClosed,
            request.OpenTime,
            request.CloseTime);

        var alreadyExists =
            await _dbContext.BusinessScheduleExceptions
                .AnyAsync(
                    x =>
                        x.BusinessId == businessId &&
                        x.Date == request.Date,
                    cancellationToken);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "Există deja o excepție de program pentru această dată.");
        }

        var scheduleException =
            new BusinessScheduleException
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Date = request.Date,
                IsClosed = request.IsClosed,
                OpenTime = request.IsClosed
                    ? null
                    : request.OpenTime,
                CloseTime = request.IsClosed
                    ? null
                    : request.CloseTime,
                Reason = NormalizeOptionalText(
                    request.Reason)
            };

        _dbContext.BusinessScheduleExceptions.Add(
            scheduleException);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapBusinessScheduleException(
            scheduleException);
    }

    public async Task<BusinessScheduleExceptionResponse>
        UpdateBusinessScheduleExceptionAsync(
            Guid userId,
            Guid businessId,
            Guid exceptionId,
            UpdateBusinessScheduleExceptionRequest request,
            CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        ValidateScheduleException(
            request.IsClosed,
            request.OpenTime,
            request.CloseTime);

        var scheduleException =
            await _dbContext.BusinessScheduleExceptions
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == exceptionId &&
                        x.BusinessId == businessId,
                    cancellationToken);

        if (scheduleException is null)
        {
            throw new KeyNotFoundException(
                "Excepția de program nu a fost găsită.");
        }

        scheduleException.IsClosed =
            request.IsClosed;

        scheduleException.OpenTime =
            request.IsClosed
                ? null
                : request.OpenTime;

        scheduleException.CloseTime =
            request.IsClosed
                ? null
                : request.CloseTime;

        scheduleException.Reason =
            NormalizeOptionalText(
                request.Reason);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapBusinessScheduleException(
            scheduleException);
    }

    public async Task DeleteBusinessScheduleExceptionAsync(
        Guid userId,
        Guid businessId,
        Guid exceptionId,
        CancellationToken cancellationToken = default)
    {
        await _businessAccess.RequireOwnerAsync(
            userId,
            businessId,
            cancellationToken);

        var scheduleException =
            await _dbContext.BusinessScheduleExceptions
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == exceptionId &&
                        x.BusinessId == businessId,
                    cancellationToken);

        if (scheduleException is null)
        {
            throw new KeyNotFoundException(
                "Excepția de program nu a fost găsită.");
        }

        _dbContext.BusinessScheduleExceptions.Remove(
            scheduleException);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
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

    private static void ValidateExceptionFilter(
        BusinessScheduleExceptionFilterRequest request)
    {
        if (request.FromDate.HasValue &&
            request.ToDate.HasValue &&
            request.FromDate.Value > request.ToDate.Value)
        {
            throw new InvalidOperationException(
                "Data de început nu poate fi după data de sfârșit.");
        }
    }

    private static void ValidateScheduleException(
        bool isClosed,
        TimeOnly? openTime,
        TimeOnly? closeTime)
    {
        if (isClosed)
        {
            if (openTime.HasValue ||
                closeTime.HasValue)
            {
                throw new InvalidOperationException(
                    "O zi în care salonul este închis nu poate avea ore de deschidere sau închidere.");
            }

            return;
        }

        if (!openTime.HasValue ||
            !closeTime.HasValue)
        {
            throw new InvalidOperationException(
                "Pentru o zi cu program special trebuie specificate ora de deschidere și ora de închidere.");
        }

        if (openTime.Value >= closeTime.Value)
        {
            throw new InvalidOperationException(
                "Ora de deschidere trebuie să fie înaintea orei de închidere.");
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

    private static BusinessScheduleExceptionResponse
        MapBusinessScheduleException(
            BusinessScheduleException scheduleException)
    {
        return new BusinessScheduleExceptionResponse
        {
            Id = scheduleException.Id,
            Date = scheduleException.Date,
            IsClosed = scheduleException.IsClosed,
            OpenTime = scheduleException.OpenTime,
            CloseTime = scheduleException.CloseTime,
            Reason = scheduleException.Reason
        };
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
}