using System.ComponentModel.DataAnnotations;
using Pufzi.Contracts.Enums;

namespace Pufzi.Contracts.Requests.Leave;

/// <summary>
/// Datele necesare pentru trimiterea unei cereri de concediu sau absență.
/// </summary>
public class CreateLeaveRequest
{
    /// <summary>
    /// Tipul concediului sau al absenței solicitate.
    /// </summary>
    [Required]
    public LeaveTypeRequest Type { get; set; }

    /// <summary>
    /// Prima zi a perioadei solicitate.
    /// </summary>
    [Required]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Ultima zi a perioadei solicitate.
    /// </summary>
    [Required]
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Motivul sau informațiile suplimentare oferite de angajat.
    /// </summary>
    [MaxLength(1000)]
    public string? Reason { get; set; }
}