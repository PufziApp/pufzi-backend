namespace Pufzi.Contracts.Responses.Schedules;

/// <summary>
/// Reprezintă o excepție de la programul normal al salonului.
/// </summary>
public class BusinessScheduleExceptionResponse
{
    /// <summary>
    /// Identificatorul unic al excepției.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Data pentru care se aplică excepția.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Indică dacă salonul este complet închis.
    /// </summary>
    public bool IsClosed { get; set; }

    /// <summary>
    /// Ora specială de deschidere, dacă salonul este deschis.
    /// </summary>
    public TimeOnly? OpenTime { get; set; }

    /// <summary>
    /// Ora specială de închidere, dacă salonul este deschis.
    /// </summary>
    public TimeOnly? CloseTime { get; set; }

    /// <summary>
    /// Motivul excepției de program.
    /// </summary>
    public string? Reason { get; set; }
}