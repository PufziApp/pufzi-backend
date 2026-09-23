namespace Pufzi.Contracts.Responses.Services;

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
    /// Frontend-ul poate folosi acest cod pentru traducere și afișare.
    /// De exemplu: dog, cat, rabbit.
    /// </summary>
    public required string Code { get; init; }
}