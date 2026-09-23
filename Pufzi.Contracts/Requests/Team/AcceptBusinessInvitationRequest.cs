using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Team;

/// <summary>
/// Datele necesare pentru acceptarea unei invitații în echipa unui salon.
/// </summary>
public class AcceptBusinessInvitationRequest
{
    /// <summary>
    /// Tokenul unic primit prin linkul invitației.
    /// </summary>
    [Required]
    public required string Token { get; set; }
}