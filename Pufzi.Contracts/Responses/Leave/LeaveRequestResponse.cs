using Pufzi.Contracts.Enums;

namespace Pufzi.Contracts.Responses.Leave;

/// <summary>
/// Informațiile unei cereri de concediu sau absență.
/// </summary>
public class LeaveRequestResponse
{
    /// <summary>
    /// Identificatorul unic al cererii.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificatorul apartenenței angajatului la salon.
    /// </summary>
    public Guid BusinessMembershipId { get; set; }

    /// <summary>
    /// Identificatorul utilizatorului care a trimis cererea.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Prenumele angajatului.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Numele de familie al angajatului.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Tipul concediului sau al absenței.
    /// </summary>
    public LeaveTypeRequest Type { get; set; }

    /// <summary>
    /// Prima zi a perioadei solicitate.
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Ultima zi a perioadei solicitate.
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Numărul efectiv de zile solicitate, calculat de backend.
    /// </summary>
    public int RequestedDays { get; set; }

    /// <summary>
    /// Motivul sau informațiile suplimentare introduse de angajat.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Statusul curent al cererii.
    /// </summary>
    public LeaveRequestStatusRequest Status { get; set; }

    /// <summary>
    /// Observația proprietarului după procesarea cererii.
    /// </summary>
    public string? ReviewNote { get; set; }

    /// <summary>
    /// Identificatorul utilizatorului care a procesat cererea.
    /// </summary>
    public Guid? ReviewedByUserId { get; set; }

    /// <summary>
    /// Data și ora la care cererea a fost procesată.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Data și ora la care cererea a fost creată.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data și ora ultimei modificări.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}