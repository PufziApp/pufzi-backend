using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Services;

/// <summary>
/// Reprezintă configurarea unui serviciu pentru o anumită specie de animal.
/// </summary>
public class CreateServiceOptionRequest
{
    /// <summary>
    /// ID-ul speciei de animal pentru care este disponibil serviciul.
    /// ID-ul trebuie să corespundă unei specii active din sistem.
    /// </summary>
    public Guid AnimalSpeciesId { get; set; }

    /// <summary>
    /// Durata implicită a serviciului pentru această specie, exprimată în minute.
    /// Opțională. Poate fi suprascrisă de durata unei variante de preț.
    /// Valoare între 1 și 1440 de minute.
    /// </summary>
    [Range(1, 1440)]
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Ordinea de afișare a acestei opțiuni în cadrul serviciului.
    /// Valorile mai mici sunt afișate primele.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Variantele de preț disponibile pentru această specie.
    /// Este necesară cel puțin o variantă de preț.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required ICollection<CreateServicePriceVariantRequest>
        PriceVariants
    { get; set; }
}