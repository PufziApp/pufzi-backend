using Pufzi.Contracts.Enums;

namespace Pufzi.Contracts.Responses.Team;

/// <summary>
/// Reprezintă o invitație trimisă unei persoane pentru a se alătura echipei salonului.
/// </summary>
public class BusinessInvitationResponse
{
    /// <summary>
    /// Identificatorul unic al invitației.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Adresa de email la care a fost trimisă invitația.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Rolul care va fi atribuit persoanei după acceptarea invitației.
    /// </summary>
    public BusinessRoleRequest Role { get; set; }

    /// <summary>
    /// Data și ora UTC la care invitația a fost creată.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data și ora UTC la care invitația expiră.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Data și ora UTC la care invitația a fost acceptată.
    /// Este null dacă invitația nu a fost acceptată.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// Data și ora UTC la care invitația a fost anulată de salon.
    /// Este null dacă invitația nu a fost anulată.
    /// </summary>
    public DateTime? RevokedAt { get; set; }
}