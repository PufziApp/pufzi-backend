using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class BusinessMembership
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid UserId { get; set; }

    public BusinessRole Role { get; set; }

    public string? JobTitle { get; set; }

    public string? Bio { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Business Business { get; set; } = null!;

    public User User { get; set; } = null!;

    public ICollection<EmployeeWorkingHour> WorkingHours { get; set; } = [];

    public ICollection<EmployeeLeaveBalance> LeaveBalances { get; set; } = [];

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
}