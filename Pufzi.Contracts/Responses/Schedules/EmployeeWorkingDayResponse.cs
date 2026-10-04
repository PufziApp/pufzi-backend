namespace Pufzi.Contracts.Responses.Schedules;

/// <summary>
/// Reprezintă programul unui membru al echipei pentru o anumită zi.
/// </summary>
public class EmployeeWorkingDayResponse
{
    /// <summary>
    /// Ziua săptămânii.
    /// </summary>
    public DayOfWeek DayOfWeek { get; init; }

    /// <summary>
    /// Indică dacă membrul echipei lucrează în această zi.
    /// </summary>
    public bool IsWorking { get; init; }

    /// <summary>
    /// Intervalele orare în care membrul echipei lucrează.
    /// </summary>
    public IReadOnlyCollection<WorkingTimeIntervalResponse> Intervals { get; init; }
        = [];
}