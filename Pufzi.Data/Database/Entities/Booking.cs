using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class Booking
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid UserId { get; set; }

    public Guid BusinessMembershipId { get; set; }

    public BookingStatus Status { get; set; } =
        BookingStatus.Pending;

    public string? Notes { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Business Business { get; set; } = null!;

    public User User { get; set; } = null!;

    public BusinessMembership BusinessMembership { get; set; } = null!;

    public User? ReviewedByUser { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<BookingChangeRequest> ChangeRequests { get; set; } = [];

    public ICollection<BookingHistory> History { get; set; } = [];
}