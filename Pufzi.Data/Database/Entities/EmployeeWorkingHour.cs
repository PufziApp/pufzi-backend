namespace Pufzi.Data.Database.Entities;

public class EmployeeWorkingHour
{
    public Guid Id { get; set; }

    public Guid BusinessMembershipId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public bool IsWorking { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public BusinessMembership BusinessMembership { get; set; } = null!;
}