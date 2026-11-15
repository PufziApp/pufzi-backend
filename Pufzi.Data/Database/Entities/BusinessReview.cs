namespace Pufzi.Data.Database.Entities;

public class BusinessReview
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public string? BusinessReply { get; set; }

    public Guid? RepliedByUserId { get; set; }

    public DateTime? RepliedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Business Business { get; set; } = null!;

    public Booking Booking { get; set; } = null!;

    public User User { get; set; } = null!;

    public User? RepliedByUser { get; set; }
}