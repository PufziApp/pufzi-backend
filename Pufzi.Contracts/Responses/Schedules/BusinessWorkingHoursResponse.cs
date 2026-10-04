namespace Pufzi.Contracts.Responses.Schedules;

/// <summary>
/// Reprezintă programul săptămânal al salonului.
/// </summary>
public class BusinessWorkingHoursResponse
{
    /// <summary>
    /// Identificatorul salonului.
    /// </summary>
    public Guid BusinessId { get; init; }

    /// <summary>
    /// Programul salonului pentru fiecare zi a săptămânii.
    /// </summary>
    public IReadOnlyCollection<BusinessWorkingDayResponse> Days { get; init; }
        = [];
}