namespace Pufzi.Contracts.Enums;

/// <summary>
/// Tipurile de concediu sau absență disponibile pentru membrii echipei.
/// </summary>
public enum LeaveTypeRequest
{
    /// <summary>
    /// Concediu de odihnă care consumă din soldul anual disponibil.
    /// </summary>
    AnnualLeave = 1,

    /// <summary>
    /// Concediu medical care nu consumă din soldul anual de concediu.
    /// </summary>
    SickLeave = 2,

    /// <summary>
    /// Concediu fără plată care nu consumă din soldul anual de concediu.
    /// </summary>
    UnpaidLeave = 3,

    /// <summary>
    /// Alt tip de absență.
    /// </summary>
    Other = 4
}