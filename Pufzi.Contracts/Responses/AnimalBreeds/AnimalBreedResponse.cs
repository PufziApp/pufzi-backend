namespace Pufzi.Contracts.Responses.AnimalBreeds;

/// <summary>
/// Informațiile publice despre o rasă de animal
/// disponibilă în nomenclatorul Pufzi.
/// </summary>
public class AnimalBreedResponse
{
    /// <summary>
    /// Identificatorul unic al rasei.
    /// Se utilizează ca AnimalBreedId la crearea
    /// sau actualizarea profilului unui animal.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identificatorul speciei căreia îi aparține rasa.
    /// Face referire la Id-ul unei specii din AnimalSpecies.
    /// </summary>
    public Guid AnimalSpeciesId { get; init; }

    /// <summary>
    /// Codul rasei utilizat pentru identificarea acesteia.
    /// Frontend-ul poate folosi codul pentru traduceri
    /// sau pentru afișarea denumirii corespunzătoare.
    /// </summary>
    public required string Code { get; init; }
}