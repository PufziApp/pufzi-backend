namespace Pufzi.Contracts.Responses.Salons;

/// <summary>
/// Informațiile rezumative ale unui salon publicat.
/// Este utilizat pentru afișarea saloanelor în liste,
/// rezultate de căutare și carduri din aplicația mobilă.
/// </summary>
public class SalonListItemResponse
{
    /// <summary>
    /// Identificatorul unic al salonului.
    /// Este același identificator ca Business.Id.
    /// Se utilizează pentru accesarea profilului public.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Numele public al salonului.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Identificatorul textual unic al salonului.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Descrierea publică a salonului, dacă există.
    /// Frontend-ul poate afișa o versiune prescurtată.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Orașul în care se află salonul.
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Județul sau regiunea în care se află salonul.
    /// </summary>
    public string? County { get; init; }

    /// <summary>
    /// Codul ISO al țării în care se află salonul.
    /// </summary>
    public string CountryCode { get; init; } = string.Empty;

    /// <summary>
    /// Adresa URL a logo-ului salonului.
    /// Poate fi null dacă salonul nu are logo.
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Adresa URL a imaginii de copertă.
    /// Poate fi null dacă salonul nu are copertă.
    /// </summary>
    public string? CoverImageUrl { get; init; }
}