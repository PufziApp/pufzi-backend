
using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Notifications.Requests;

/// <summary>
/// Reprezintă datele necesare pentru înregistrarea
/// sau actualizarea unui dispozitiv pentru notificări push.
/// </summary>
public class RegisterDeviceRequest
{
    /// <summary>
    /// Tokenul Firebase Cloud Messaging al dispozitivului.
    /// </summary>
    [Required]
    [StringLength(4096, MinimumLength = 1)]
    public required string FcmToken { get; set; }

    /// <summary>
    /// Platforma dispozitivului: ios sau android.
    /// </summary>
    [Required]
    [RegularExpression("^(ios|android)$")]
    public required string Platform { get; set; }

    /// <summary>
    /// Limba selectată în aplicație: ro, en sau hu.
    /// </summary>
    [Required]
    [RegularExpression("^(ro|en|hu)$")]
    public required string Language { get; set; }
}
