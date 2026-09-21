namespace Pufzi.Contracts.Responses.Services;

public class AnimalSpeciesResponse
{
    public Guid Id { get; init; }

    public required string Code { get; init; }
}