
namespace Pufzi.Contracts.Notifications.Responses;

/// <summary>
/// Reprezintă un dispozitiv înregistrat pentru
/// notificări push.
/// </summary>
public class UserDeviceResponse
{
    public Guid Id { get; set; }

    public string Platform { get; set; } = string.Empty;

    public string Language { get; set; } = "ro";

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastSeenAt { get; set; }
}
