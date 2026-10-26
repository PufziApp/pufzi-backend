using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class BookingChangeRequest
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid RequestedByUserId { get; set; }

    public BookingChangeRequestStatus Status { get; set; } =
        BookingChangeRequestStatus.Pending;

    public string? Reason { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Booking Booking { get; set; } = null!;

    public User RequestedByUser { get; set; } = null!;

    public User? ReviewedByUser { get; set; }

    public ICollection<BookingChangeAppointment> Appointments { get; set; } = [];
}