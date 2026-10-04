namespace Pufzi.Data.Database.Entities;

public class LeaveRequestBalanceUsage
{
    public Guid Id { get; set; }

    public Guid LeaveRequestId { get; set; }

    public int Year { get; set; }

    public int Days { get; set; }

    public LeaveRequest LeaveRequest { get; set; } = null!;
}