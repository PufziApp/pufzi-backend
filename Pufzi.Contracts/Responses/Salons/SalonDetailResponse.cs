using Pufzi.Contracts.Responses.Businesses;

namespace Pufzi.Contracts.Responses.Salons;

/// <summary>
/// Profilul public complet al unui salon Pufzi.
/// Este utilizat pentru afișarea paginii de detalii a salonului.
/// </summary>
/// <remarks>
/// Sunt returnate doar informațiile destinate afișării publice.
/// Endpoint-ul care utilizează acest răspuns este accesibil
/// fără autentificare și returnează doar saloane publicate.
/// </remarks>
public class SalonDetailResponse
{
    /// <summary>
    /// Identificatorul unic al salonului.
    /// Este același identificator ca Business.Id.
    /// Se utilizează la solicitarea serviciilor,
    /// pachetelor și, ulterior, a disponibilității salonului.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Numele public al salonului.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Identificatorul textual unic al salonului.
    /// Poate fi utilizat pentru construirea unor URL-uri prietenoase.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Descrierea publică a salonului.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Numărul de telefon de contact al salonului.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Adresa de email de contact a salonului.
    /// </summary>
    public string? ContactEmail { get; init; }

    /// <summary>
    /// Adresa fizică a salonului: stradă, număr și alte detalii.
    /// </summary>
    public string? AddressLine { get; init; }

    /// <summary>
    /// Orașul în care se află salonul.
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Județul sau regiunea în care se află salonul.
    /// </summary>
    public string? County { get; init; }

    /// <summary>
    /// Codul poștal al salonului.
    /// </summary>
    public string? PostalCode { get; init; }

    /// <summary>
    /// Codul ISO al țării în care se află salonul.
    /// </summary>
    public string CountryCode { get; init; } = string.Empty;

    /// <summary>
    /// Identificatorul fusului orar al salonului.
    /// Va fi utilizat pentru interpretarea corectă
    /// a programului de lucru și a orelor disponibile.
    /// </summary>
    public string TimeZoneId { get; init; } = string.Empty;

    /// <summary>
    /// Latitudinea geografică a salonului, dacă este configurată.
    /// Se utilizează pentru poziționarea pe hartă.
    /// </summary>
    public decimal? Latitude { get; init; }

    /// <summary>
    /// Longitudinea geografică a salonului, dacă este configurată.
    /// Se utilizează pentru poziționarea pe hartă.
    /// </summary>
    public decimal? Longitude { get; init; }

    /// <summary>
    /// Adresa URL a logo-ului salonului.
    /// Este null dacă salonul nu are logo.
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Adresa URL a imaginii de copertă a salonului.
    /// Este null dacă salonul nu are imagine de copertă.
    /// </summary>
    public string? CoverImageUrl { get; init; }

    /// <summary>
    /// Linkul către profilul Instagram al salonului, dacă există.
    /// </summary>
    public string? InstagramUrl { get; init; }

    /// <summary>
    /// Linkul către pagina Facebook a salonului, dacă există.
    /// </summary>
    public string? FacebookUrl { get; init; }

    /// <summary>
    /// Fotografiile publice din galeria salonului.
    /// Fiecare element conține informațiile definite
    /// în BusinessImageResponse.
    /// </summary>
    public IReadOnlyCollection<BusinessImageResponse> Images { get; init; }
        = [];
}