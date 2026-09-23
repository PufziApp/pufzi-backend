using System.ComponentModel.DataAnnotations;
using Pufzi.Contracts.Enums;

namespace Pufzi.Contracts.Requests.Team;

/// <summary>
/// Datele necesare pentru invitarea unui nou membru în echipa salonului.
/// </summary>
public class CreateBusinessInvitationRequest
{
    /// <summary>
    /// Adresa de email a persoanei care va primi invitația.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public required string Email { get; set; }

    /// <summary>
    /// Rolul pe care persoana îl va primi automat în salon
    /// după acceptarea invitației.
    /// </summary>
    [Required]
    public BusinessRoleRequest Role { get; set; }
}