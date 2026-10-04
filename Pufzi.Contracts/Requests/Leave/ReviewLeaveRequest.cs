using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Leave;

/// <summary>
/// Datele opționale introduse de proprietar la procesarea
/// unei cereri de concediu sau absență.
/// </summary>
public class ReviewLeaveRequest
{
    /// <summary>
    /// Observația proprietarului privind aprobarea sau respingerea cererii.
    /// </summary>
    [MaxLength(1000)]
    public string? ReviewNote { get; set; }
}