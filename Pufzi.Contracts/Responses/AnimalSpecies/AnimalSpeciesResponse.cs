namespace Pufzi.Contracts.Responses.AnimalSpecies;

/// <summary>
/// Reprezintă o specie de animal disponibilă în sistem.
/// </summary>
public class AnimalSpeciesResponse
{
    /// <summary>
    /// ID-ul unic al speciei.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Codul unic al speciei.
    /// Frontend-ul îl poate utiliza pentru traducere și afișare,
    /// de exemplu: dog, cat sau rabbit.
    /// </summary>
    public required string Code { get; init; }
}