using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Services;

/// <summary>
/// Reprezintă o variantă de preț a unui serviciu,
/// de exemplu Talie mică, Talie medie sau Talie mare.
/// </summary>
public class CreateServicePriceVariantRequest
{
    /// <summary>
    /// Numele variantei de preț.
    /// De exemplu: Talie mică, Talie medie, Talie mare sau Standard.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// Greutatea minimă a animalului pentru această variantă, exprimată în kilograme.
    /// Opțională.
    /// </summary>
    [Range(0, 9999)]
    public decimal? MinWeightKg { get; set; }

    /// <summary>
    /// Greutatea maximă a animalului pentru această variantă, exprimată în kilograme.
    /// Opțională.
    /// </summary>
    [Range(0, 9999)]
    public decimal? MaxWeightKg { get; set; }

    /// <summary>
    /// Prețul serviciului pentru această variantă.
    /// Trebuie lăsat necompletat dacă IsPriceOnRequest este true.
    /// </summary>
    [Range(0, 999999)]
    public decimal? Price { get; set; }

    /// <summary>
    /// Indică dacă prețul reprezintă un preț de pornire,
    /// de exemplu „de la 150 lei”.
    /// </summary>
    public bool IsStartingPrice { get; set; }

    /// <summary>
    /// Indică dacă prețul este stabilit la cerere.
    /// Dacă este true, Price trebuie să fie null.
    /// </summary>
    public bool IsPriceOnRequest { get; set; }

    /// <summary>
    /// Durata specifică acestei variante, exprimată în minute.
    /// Opțională. Dacă este completată, poate suprascrie durata
    /// implicită configurată pentru specie.
    /// Valoare între 1 și 1440 de minute.
    /// </summary>
    [Range(1, 1440)]
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Ordinea de afișare a variantei de preț.
    /// Valorile mai mici sunt afișate primele.
    /// </summary>
    public int SortOrder { get; set; }
}