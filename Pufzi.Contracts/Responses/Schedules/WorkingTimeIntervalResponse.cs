namespace Pufzi.Contracts.Responses.Schedules;

/// <summary>
/// Reprezintă un interval orar din programul de lucru.
/// </summary>
public class WorkingTimeIntervalResponse
{
    /// <summary>
    /// Ora locală la care începe intervalul.
    /// </summary>
    public TimeOnly StartTime { get; init; }

    /// <summary>
    /// Ora locală la care se termină intervalul.
    /// </summary>
    public TimeOnly EndTime { get; init; }
}