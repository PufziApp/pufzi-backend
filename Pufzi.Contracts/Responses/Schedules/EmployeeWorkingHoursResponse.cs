namespace Pufzi.Contracts.Responses.Schedules;

/// <summary>
/// Reprezintă programul săptămânal al unui membru al echipei.
/// </summary>
public class EmployeeWorkingHoursResponse
{
    /// <summary>
    /// Identificatorul apartenenței membrului la salon.
    /// </summary>
    public Guid BusinessMembershipId { get; init; }

    /// <summary>
    /// Identificatorul utilizatorului.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Prenumele membrului echipei.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Numele membrului echipei.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Programul membrului pentru fiecare zi a săptămânii.
    /// </summary>
    public IReadOnlyCollection<EmployeeWorkingDayResponse> Days { get; init; }
        = [];
}