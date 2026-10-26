using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Salons;

/// <summary>
/// Parametrii opționali pentru căutarea, filtrarea și paginarea
/// saloanelor publicate în aplicația Pufzi.
/// </summary>
/// <remarks>
/// Toate filtrele sunt opționale.
/// Dacă nu sunt transmise filtre, sunt returnate saloanele publicate,
/// în limita paginării.
/// </remarks>
public class SalonSearchRequest
{
    /// <summary>
    /// Textul căutat în numele salonului.
    /// </summary>
    /// <example>Happy Paws</example>
    public string? Search { get; set; }

    /// <summary>
    /// Orașul în care se află salonul.
    /// </summary>
    /// <example>Satu Mare</example>
    public string? City { get; set; }

    /// <summary>
    /// Județul sau regiunea în care se află salonul.
    /// </summary>
    /// <example>Satu Mare</example>
    public string? County { get; set; }

    /// <summary>
    /// Codul ISO al țării în care se află salonul.
    /// </summary>
    /// <example>RO</example>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Numărul paginii solicitate. Numerotarea începe de la 1.
    /// </summary>
    /// <example>1</example>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Numărul de saloane returnate pe o pagină.
    /// Valoarea implicită este 20.
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; } = 20;
}