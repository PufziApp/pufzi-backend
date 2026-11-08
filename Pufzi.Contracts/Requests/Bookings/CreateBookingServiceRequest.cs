namespace Pufzi.Contracts.Requests.Bookings;

/// <summary>
/// Pachetul de servicii selectat pentru un animal.
/// </summary>
public class CreateBookingServiceRequest
{
    /// <summary>
    /// Identificatorul pachetului de servicii.
    /// </summary>
    public Guid ServicePackageId { get; set; }

    /// <summary>
    /// Identificatorii variantelor selectate pentru serviciile din pachet.
    /// </summary>
    public List<Guid> ServicePackageItemVariantIds { get; set; } = [];
}