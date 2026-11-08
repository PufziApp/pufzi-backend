using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Entities;

public class BookingHistory
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid? PerformedByUserId { get; set; }

    public BookingHistoryAction Action { get; set; }

    public BookingStatus? PreviousStatus { get; set; }

    public BookingStatus? NewStatus { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Booking Booking { get; set; } = null!;

    public User? PerformedByUser { get; set; }
}