using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class LeaveRequest
{
    public Guid Id { get; set; }

    public Guid BusinessMembershipId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int RequestedDays { get; set; }

    public string? Reason { get; set; }

    public LeaveRequestStatus Status { get; set; }
        = LeaveRequestStatus.Pending;

    public Guid? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public BusinessMembership BusinessMembership { get; set; } = null!;

    public User? ReviewedByUser { get; set; }
}