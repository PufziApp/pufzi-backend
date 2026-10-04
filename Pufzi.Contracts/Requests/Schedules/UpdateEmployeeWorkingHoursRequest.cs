namespace Pufzi.Contracts.Requests.Schedules;

/// <summary>
/// Conține programul săptămânal complet al unui membru al echipei.
/// </summary>
public class UpdateEmployeeWorkingHoursRequest
{
    /// <summary>
    /// Programul membrului pentru zilele săptămânii.
    /// Programul existent este înlocuit cu valorile trimise.
    /// </summary>
    public IReadOnlyCollection<EmployeeWorkingDayRequest> Days { get; init; }
        = [];
}