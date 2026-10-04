using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Schedules;

/// <summary>
/// Datele care pot fi modificate pentru o excepție
/// de program existentă.
/// </summary>
public class UpdateBusinessScheduleExceptionRequest
{
    /// <summary>
    /// Indică dacă salonul este complet închis în această zi.
    /// </summary>
    [Required]
    public bool IsClosed { get; set; }

    /// <summary>
    /// Ora specială de deschidere.
    /// Este utilizată doar dacă salonul nu este închis.
    /// </summary>
    public TimeOnly? OpenTime { get; set; }

    /// <summary>
    /// Ora specială de închidere.
    /// Este utilizată doar dacă salonul nu este închis.
    /// </summary>
    public TimeOnly? CloseTime { get; set; }

    /// <summary>
    /// Motivul excepției de program.
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}