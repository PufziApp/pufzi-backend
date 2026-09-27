using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.ServicePackages;

/// <summary>
/// Reprezintă o variantă de preț și durată
/// pentru un serviciu inclus într-un pachet.
/// </summary>
public class ServicePackageItemVariantRequest
{
    /// <summary>
    /// Numele variantei.
    /// De exemplu: Talie mică, Talie medie sau Talie mare.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    /// <summary>
    /// Greutatea minimă pentru această variantă, exprimată în kilograme.
    /// Poate fi null.
    /// </summary>
    [Range(0, 1000)]
    public decimal? MinWeightKg { get; init; }

    /// <summary>
    /// Greutatea maximă pentru această variantă, exprimată în kilograme.
    /// Poate fi null.
    /// </summary>
    [Range(0, 1000)]
    public decimal? MaxWeightKg { get; init; }

    /// <summary>
    /// Prețul variantei.
    /// Poate fi null atunci când prețul este disponibil la cerere.
    /// </summary>
    [Range(0, 1_000_000)]
    public decimal? Price { get; init; }

    /// <summary>
    /// Indică dacă prețul reprezintă un preț de pornire,
    /// de exemplu „de la 150 RON”.
    /// </summary>
    public bool IsStartingPrice { get; init; }

    /// <summary>
    /// Indică dacă prețul este disponibil doar la cerere.
    /// </summary>
    public bool IsPriceOnRequest { get; init; }

    /// <summary>
    /// Durata aproximativă a serviciului pentru această variantă,
    /// exprimată în minute.
    /// Poate fi null.
    /// </summary>
    [Range(1, 1440)]
    public int? DurationMinutes { get; init; }

    /// <summary>
    /// Ordinea de afișare a variantei.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int SortOrder { get; init; }
}