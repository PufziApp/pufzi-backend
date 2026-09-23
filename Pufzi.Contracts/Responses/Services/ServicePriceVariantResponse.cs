namespace Pufzi.Contracts.Responses.Services;

/// <summary>
/// Reprezintă o variantă de preț disponibilă pentru un serviciu.
/// </summary>
public class ServicePriceVariantResponse
{
    /// <summary>
    /// ID-ul unic al variantei de preț.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Numele variantei.
    /// De exemplu: Talie mică, Talie medie sau Talie mare.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Greutatea minimă asociată variantei, exprimată în kilograme.
    /// Poate fi null.
    /// </summary>
    public decimal? MinWeightKg { get; init; }

    /// <summary>
    /// Greutatea maximă asociată variantei, exprimată în kilograme.
    /// Poate fi null.
    /// </summary>
    public decimal? MaxWeightKg { get; init; }

    /// <summary>
    /// Prețul variantei.
    /// Este null dacă prețul este disponibil doar la cerere.
    /// </summary>
    public decimal? Price { get; init; }

    /// <summary>
    /// Indică dacă prețul este un preț de pornire,
    /// de exemplu „de la 150 lei”.
    /// </summary>
    public bool IsStartingPrice { get; init; }

    /// <summary>
    /// Indică dacă prețul este disponibil la cerere.
    /// </summary>
    public bool IsPriceOnRequest { get; init; }

    /// <summary>
    /// Durata specifică acestei variante, exprimată în minute.
    /// Poate fi null.
    /// </summary>
    public int? DurationMinutes { get; init; }

    /// <summary>
    /// Ordinea de afișare a variantei.
    /// </summary>
    public int SortOrder { get; init; }
}