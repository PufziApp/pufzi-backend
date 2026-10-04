namespace Pufzi.Contracts.Responses.Schedules;

/// <summary>
/// Reprezintă programul salonului pentru o anumită zi.
/// </summary>
public class BusinessWorkingDayResponse
{
    /// <summary>
    /// Ziua săptămânii.
    /// </summary>
    public DayOfWeek DayOfWeek { get; init; }

    /// <summary>
    /// Indică dacă salonul este deschis.
    /// </summary>
    public bool IsOpen { get; init; }

    /// <summary>
    /// Intervalele orare în care salonul este deschis.
    /// </summary>
    public IReadOnlyCollection<WorkingTimeIntervalResponse> Intervals { get; init; }
        = [];
}