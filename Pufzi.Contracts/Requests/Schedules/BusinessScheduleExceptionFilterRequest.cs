namespace Pufzi.Contracts.Requests.Schedules;

/// <summary>
/// Filtrele utilizate pentru obținerea excepțiilor
/// de program ale salonului.
/// </summary>
public class BusinessScheduleExceptionFilterRequest
{
    /// <summary>
    /// Prima dată inclusă în perioada solicitată.
    /// </summary>
    public DateOnly? FromDate { get; set; }

    /// <summary>
    /// Ultima dată inclusă în perioada solicitată.
    /// </summary>
    public DateOnly? ToDate { get; set; }
}