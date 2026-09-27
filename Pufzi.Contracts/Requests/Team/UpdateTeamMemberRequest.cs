using System.ComponentModel.DataAnnotations;
using Pufzi.Contracts.Enums;

namespace Pufzi.Contracts.Requests.Team;

/// <summary>
/// Datele care pot fi modificate de proprietarul salonului
/// pentru un membru al echipei.
/// </summary>
public class UpdateTeamMemberRequest
{
    /// <summary>
    /// Rolul membrului în cadrul salonului.
    /// Rolul Owner nu poate fi atribuit prin această operație.
    /// </summary>
    [Required]
    public BusinessRoleRequest Role { get; set; }

    /// <summary>
    /// Titlul profesional afișat în profilul membrului.
    /// </summary>
    [MaxLength(100)]
    public string? JobTitle { get; set; }

    /// <summary>
    /// Descrierea profesională afișată în profilul membrului.
    /// </summary>
    [MaxLength(1000)]
    public string? Bio { get; set; }
}