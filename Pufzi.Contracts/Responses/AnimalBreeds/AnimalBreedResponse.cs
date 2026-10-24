namespace Pufzi.Contracts.Responses.AnimalBreeds;

/// <summary>
/// O rasă disponibilă pentru o specie.
/// </summary>
public class AnimalBreedResponse
{
    public Guid Id { get; init; }

    public Guid AnimalSpeciesId { get; init; }

    public required string Code { get; init; }
}