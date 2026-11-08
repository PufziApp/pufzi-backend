using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Responses.Salons.Availability;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.Bookings;

public class AppointmentAvailabilityService
    : IAppointmentAvailabilityService
{
    private const int SlotIntervalMinutes = 30;
    private const int MaxDurationMinutes = 720;

    private readonly PufziDbContext _dbContext;

    public AppointmentAvailabilityService(
        PufziDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SalonAvailabilityResponse> GetAvailabilityAsync(
        Guid salonId,
        Guid businessMembershipId,
        DateOnly date,
        int durationMinutes,
        CancellationToken cancellationToken = default)
    {
        if (durationMinutes <= 0 ||
            durationMinutes > MaxDurationMinutes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationMinutes),
                "Appointment duration must be between 1 and 720 minutes.");
        }

        var salon = await _dbContext.Businesses
            .AsNoTracking()
            .Where(x =>
                x.Id == salonId &&
                x.IsPublished)
            .Select(x => new
            {
                x.Id,
                x.TimeZoneId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (salon is null)
        {
            throw new KeyNotFoundException(
                "Salon not found.");
        }

        var employeeExists = await _dbContext.BusinessMemberships
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == businessMembershipId &&
                x.BusinessId == salonId &&
                x.IsActive &&
                x.RemovedAt == null &&
                x.User.IsActive,
                cancellationToken);

        if (!employeeExists)
        {
            throw new KeyNotFoundException(
                "Employee not found.");
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            salon.TimeZoneId);

        var response = new SalonAvailabilityResponse
        {
            SalonId = salonId,
            BusinessMembershipId = businessMembershipId,
            Date = date,
            TimeZoneId = salon.TimeZoneId,
            DurationMinutes = durationMinutes
        };

        var businessHours = await _dbContext.BusinessWorkingHours
            .AsNoTracking()
            .Where(x =>
                x.BusinessId == salonId &&
                x.DayOfWeek == date.DayOfWeek &&
                x.IsOpen &&
                x.StartTime.HasValue &&
                x.EndTime.HasValue)
            .Select(x => new
            {
                Start = x.StartTime!.Value,
                End = x.EndTime!.Value
            })
            .ToListAsync(cancellationToken);

        var scheduleException =
            await _dbContext.BusinessScheduleExceptions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == salonId &&
                    x.Date == date,
                    cancellationToken);

        if (scheduleException is not null)
        {
            if (scheduleException.IsClosed)
            {
                return response;
            }

            if (scheduleException.OpenTime.HasValue &&
                scheduleException.CloseTime.HasValue)
            {
                businessHours =
                [
                    new
                    {
                        Start = scheduleException.OpenTime.Value,
                        End = scheduleException.CloseTime.Value
                    }
                ];
            }
        }

        var employeeHours = await _dbContext.EmployeeWorkingHours
            .AsNoTracking()
            .Where(x =>
                x.BusinessMembershipId == businessMembershipId &&
                x.DayOfWeek == date.DayOfWeek &&
                x.IsWorking &&
                x.StartTime.HasValue &&
                x.EndTime.HasValue)
            .Select(x => new
            {
                Start = x.StartTime!.Value,
                End = x.EndTime!.Value
            })
            .ToListAsync(cancellationToken);

        if (businessHours.Count == 0 ||
            employeeHours.Count == 0)
        {
            return response;
        }

        var hasApprovedLeave = await _dbContext.LeaveRequests
            .AsNoTracking()
            .AnyAsync(x =>
                x.BusinessMembershipId == businessMembershipId &&
                x.Status == LeaveRequestStatus.Approved &&
                x.StartDate <= date &&
                x.EndDate >= date,
                cancellationToken);

        if (hasApprovedLeave)
        {
            return response;
        }

        var localDayStart = date.ToDateTime(
            TimeOnly.MinValue,
            DateTimeKind.Unspecified);

        var searchStartUtc = DateTime.SpecifyKind(
            localDayStart.AddDays(-2),
            DateTimeKind.Utc);

        var searchEndUtc = DateTime.SpecifyKind(
            localDayStart.AddDays(2),
            DateTimeKind.Utc);

        var occupiedSlots = await _dbContext.AppointmentSlots
            .AsNoTracking()
            .Where(x =>
                x.BusinessMembershipId == businessMembershipId &&
                x.StartAt < searchEndUtc &&
                x.EndAt > searchStartUtc)
            .Select(x => new
            {
                x.StartAt,
                x.EndAt
            })
            .ToListAsync(cancellationToken);

        var nowUtc = DateTime.UtcNow;

        for (var minutes = 0;
             minutes < 24 * 60;
             minutes += SlotIntervalMinutes)
        {
            var localStart = localDayStart.AddMinutes(minutes);
            var localEnd = localStart.AddMinutes(durationMinutes);

            // Appointments must fit inside the same local day.
            if (localStart.Date != localEnd.Date)
            {
                continue;
            }

            var startTime = TimeOnly.FromDateTime(localStart);
            var endTime = TimeOnly.FromDateTime(localEnd);

            var salonIsOpen = businessHours.Any(x =>
                x.Start <= startTime &&
                x.End >= endTime);

            if (!salonIsOpen)
            {
                continue;
            }

            var employeeIsWorking = employeeHours.Any(x =>
                x.Start <= startTime &&
                x.End >= endTime);

            if (!employeeIsWorking)
            {
                continue;
            }

            // Skip invalid and ambiguous times during DST changes.
            if (timeZone.IsInvalidTime(localStart) ||
                timeZone.IsInvalidTime(localEnd) ||
                timeZone.IsAmbiguousTime(localStart) ||
                timeZone.IsAmbiguousTime(localEnd))
            {
                continue;
            }

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(
                localStart,
                timeZone);

            var endUtc = TimeZoneInfo.ConvertTimeToUtc(
                localEnd,
                timeZone);

            if (startUtc <= nowUtc ||
                endUtc <= startUtc)
            {
                continue;
            }

            var overlaps = occupiedSlots.Any(x =>
                startUtc < x.EndAt &&
                endUtc > x.StartAt);

            if (overlaps)
            {
                continue;
            }

            response.AvailableStartTimesUtc.Add(startUtc);
        }

        return response;
    }
}