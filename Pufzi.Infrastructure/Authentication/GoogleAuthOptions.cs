namespace Pufzi.Infrastructure.Authentication;

public class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    public required string WebClientId { get; init; }

    public required string AndroidClientId { get; init; }

    public required string IosClientId { get; init; }
}