namespace Pufzi.Contracts.Requests.Schedules;

/// <summary>
/// Conține programul săptămânal complet al salonului.
/// </summary>
public class UpdateBusinessWorkingHoursRequest
{
    /// <summary>
    /// Programul salonului pentru zilele săptămânii.
    /// Programul existent este înlocuit cu valorile trimise.
    /// </summary>
    public IReadOnlyCollection<BusinessWorkingDayRequest> Days { get; init; }
        = [];
}