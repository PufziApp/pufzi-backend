using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Leave;

/// <summary>
/// Datele necesare pentru configurarea soldului anual de concediu
/// al unui membru al echipei.
/// </summary>
public class UpdateEmployeeLeaveBalanceRequest
{
    /// <summary>
    /// Anul pentru care se configurează soldul de concediu.
    /// </summary>
    [Range(2000, 2100)]
    public int Year { get; set; }

    /// <summary>
    /// Numărul total de zile de concediu de odihnă disponibile în anul respectiv.
    /// </summary>
    [Range(0, 366)]
    public int TotalDays { get; set; }
}