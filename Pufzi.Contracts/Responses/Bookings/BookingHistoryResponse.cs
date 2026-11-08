
namespace Pufzi.Contracts.Responses.Bookings;

public class BookingHistoryResponse
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid? PerformedByUserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? PreviousStatus { get; set; }

    public string? NewStatus { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}
