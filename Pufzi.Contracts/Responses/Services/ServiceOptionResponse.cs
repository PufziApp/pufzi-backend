namespace Pufzi.Contracts.Responses.Services;

/// <summary>
/// Reprezintă configurarea unui serviciu pentru o anumită specie de animal.
/// </summary>
public class ServiceOptionResponse
{
    /// <summary>
    /// ID-ul unic al opțiunii serviciului.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Specia de animal pentru care este valabilă această opțiune.
    /// </summary>
    public required AnimalSpeciesResponse AnimalSpecies { get; init; }

    /// <summary>
    /// Durata implicită a serviciului pentru această specie,
    /// exprimată în minute.
    /// Poate fi null.
    /// </summary>
    public int? DurationMinutes { get; init; }

    /// <summary>
    /// Ordinea de afișare a opțiunii.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Variantele de preț disponibile pentru această specie.
    /// </summary>
    public required IReadOnlyCollection<ServicePriceVariantResponse>
        PriceVariants
    { get; init; }
}