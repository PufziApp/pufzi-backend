namespace Pufzi.Contracts.Responses.ServicePackages;

/// <summary>
/// Reprezintă o specie de animal pentru care este disponibil un pachet.
/// </summary>
public class ServicePackageAnimalSpeciesResponse
{
    /// <summary>
    /// ID-ul unic al speciei.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Codul unic al speciei.
    /// Frontend-ul îl poate folosi pentru traducere și afișare.
    /// De exemplu: dog, cat sau rabbit.
    /// </summary>
    public required string Code { get; init; }
}