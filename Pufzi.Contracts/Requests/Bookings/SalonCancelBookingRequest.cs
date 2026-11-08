
using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Bookings;

public class SalonCancelBookingRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}
