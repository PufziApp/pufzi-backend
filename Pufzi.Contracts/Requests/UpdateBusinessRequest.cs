using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Businesses;

public class UpdateBusinessRequest
{
    /// <summary>
    /// Numele salonului. Obligatoriu.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }

    /// <summary>
    /// Descrierea salonului. Opțional.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; init; }

    /// <summary>
    /// Numărul de telefon al salonului. Opțional.
    /// </summary>
    [MaxLength(30)]
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Adresa de email de contact. Opțional. Trebuie să fie un email valid.
    /// </summary>
    [EmailAddress]
    [MaxLength(320)]
    public string? ContactEmail { get; init; }

    /// <summary>
    /// Adresa fizică a salonului. Opțional.
    /// </summary>
    [MaxLength(300)]
    public string? AddressLine { get; init; }

    /// <summary>
    /// Orașul în care se află salonul. Opțional.
    /// </summary>
    [MaxLength(100)]
    public string? City { get; init; }

    /// <summary>
    /// Județul în care se află salonul. Opțional.
    /// </summary>
    [MaxLength(100)]
    public string? County { get; init; }

    /// <summary>
    /// Codul poștal al salonului. Opțional.
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; init; }

    /// <summary>
    /// Latitudinea salonului. Opțional. Valoare între -90 și 90.
    /// </summary>
    [Range(
        typeof(decimal),
        "-90",
        "90")]
    public decimal? Latitude { get; init; }

    /// <summary>
    /// Longitudinea salonului. Opțional. Valoare între -180 și 180.
    /// </summary>
    [Range(
        typeof(decimal),
        "-180",
        "180")]
    public decimal? Longitude { get; init; }

    /// <summary>
    /// Link către profilul de Instagram. Opțional.
    /// </summary>
    [Url]
    [MaxLength(500)]
    public string? InstagramUrl { get; init; }

    /// <summary>
    /// Link către pagina de Facebook. Opțional.
    /// </summary>
    [Url]
    [MaxLength(500)]
    public string? FacebookUrl { get; init; }
}