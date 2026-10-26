namespace Pufzi.Contracts.Responses.Salons.Availability;

public class SalonAvailabilityResponse
{
    /// <summary>
    /// The salon identifier.
    /// </summary>
    public Guid SalonId { get; set; }

    /// <summary>
    /// The assigned employee membership identifier.
    /// </summary>
    public Guid BusinessMembershipId { get; set; }

    /// <summary>
    /// The requested date in the salon's local time zone.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The salon's IANA time zone identifier.
    /// </summary>
    public required string TimeZoneId { get; set; }

    /// <summary>
    /// The required appointment duration in minutes.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Available appointment start times in UTC.
    /// </summary>
    public List<DateTime> AvailableStartTimesUtc { get; set; } = [];
}