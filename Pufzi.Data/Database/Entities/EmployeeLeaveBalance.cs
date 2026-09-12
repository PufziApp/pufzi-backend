namespace Pufzi.Data.Database.Entities;

public class EmployeeLeaveBalance
{
    public Guid Id { get; set; }

    public Guid BusinessMembershipId { get; set; }

    public int Year { get; set; }

    public int TotalDays { get; set; }

    public int UsedDays { get; set; }

    public BusinessMembership BusinessMembership { get; set; } = null!;
}