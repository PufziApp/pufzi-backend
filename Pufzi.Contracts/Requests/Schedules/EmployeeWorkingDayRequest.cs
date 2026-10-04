namespace Pufzi.Contracts.Requests.Schedules;

/// <summary>
/// Reprezintă programul unui membru al echipei pentru o anumită zi.
/// </summary>
public class EmployeeWorkingDayRequest
{
    /// <summary>
    /// Ziua săptămânii pentru care este definit programul.
    /// </summary>
    public DayOfWeek DayOfWeek { get; init; }

    /// <summary>
    /// Indică dacă membrul echipei lucrează în această zi.
    /// </summary>
    public bool IsWorking { get; init; }

    /// <summary>
    /// Intervalele orare în care membrul echipei lucrează.
    /// Pentru o zi nelucrătoare lista trebuie să fie goală.
    /// </summary>
    public IReadOnlyCollection<WorkingTimeIntervalRequest> Intervals { get; init; }
        = [];
}