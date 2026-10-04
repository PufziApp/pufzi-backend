namespace Pufzi.Contracts.Responses.Leave;

/// <summary>
/// Soldul anual de concediu de odihnă al unui membru al echipei.
/// </summary>
public class EmployeeLeaveBalanceResponse
{
    /// <summary>
    /// Identificatorul apartenenței membrului la salon.
    /// </summary>
    public Guid BusinessMembershipId { get; set; }

    /// <summary>
    /// Anul pentru care este afișat soldul.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Numărul total de zile de concediu de odihnă disponibile.
    /// </summary>
    public int TotalDays { get; set; }

    /// <summary>
    /// Numărul de zile consumate prin concedii aprobate.
    /// </summary>
    public int UsedDays { get; set; }

    /// <summary>
    /// Numărul de zile de concediu de odihnă rămase disponibile.
    /// </summary>
    public int RemainingDays { get; set; }
}