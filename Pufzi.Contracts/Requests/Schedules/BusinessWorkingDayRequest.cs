namespace Pufzi.Contracts.Requests.Schedules;

/// <summary>
/// Reprezintă programul salonului pentru o anumită zi a săptămânii.
/// </summary>
public class BusinessWorkingDayRequest
{
    /// <summary>
    /// Ziua săptămânii pentru care este definit programul.
    /// </summary>
    public DayOfWeek DayOfWeek { get; init; }

    /// <summary>
    /// Indică dacă salonul este deschis în această zi.
    /// </summary>
    public bool IsOpen { get; init; }

    /// <summary>
    /// Intervalele orare în care salonul este deschis.
    /// Pentru o zi închisă lista trebuie să fie goală.
    /// </summary>
    public IReadOnlyCollection<WorkingTimeIntervalRequest> Intervals { get; init; }
        = [];
}