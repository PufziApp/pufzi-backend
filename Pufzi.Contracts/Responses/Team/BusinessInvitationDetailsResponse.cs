using Pufzi.Contracts.Enums;

namespace Pufzi.Contracts.Responses.Team;

/// <summary>
/// Informațiile publice necesare pentru afișarea și validarea unei invitații în echipa unui salon.
/// </summary>
public class BusinessInvitationDetailsResponse
{
    /// <summary>
    /// Numele salonului care a trimis invitația.
    /// </summary>
    public required string BusinessName { get; set; }

    /// <summary>
    /// Adresa de email pentru care a fost creată invitația.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Rolul care va fi atribuit după acceptarea invitației.
    /// </summary>
    public BusinessRoleRequest Role { get; set; }

    /// <summary>
    /// Data și ora UTC la care invitația expiră.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}