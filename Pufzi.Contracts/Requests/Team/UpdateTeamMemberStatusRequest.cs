namespace Pufzi.Contracts.Requests.Team;

/// <summary>
/// Datele necesare pentru activarea sau dezactivarea
/// unui membru al echipei.
/// </summary>
public class UpdateTeamMemberStatusRequest
{
    /// <summary>
    /// Indică dacă membrul trebuie să fie activ
    /// în cadrul salonului.
    /// </summary>
    public bool IsActive { get; set; }
}