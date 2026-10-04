namespace Pufzi.Contracts.Requests.Schedules;

/// <summary>
/// Reprezintă un interval orar din programul de lucru.
/// </summary>
public class WorkingTimeIntervalRequest
{
    /// <summary>
    /// Ora locală la care începe intervalul de lucru.
    /// </summary>
    public TimeOnly StartTime { get; init; }

    /// <summary>
    /// Ora locală la care se termină intervalul de lucru.
    /// Trebuie să fie ulterioară orei de început.
    /// </summary>
    public TimeOnly EndTime { get; init; }
}