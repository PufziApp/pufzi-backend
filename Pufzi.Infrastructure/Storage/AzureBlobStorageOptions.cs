namespace Pufzi.Infrastructure.Storage;

public class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";

    public required string ConnectionString { get; init; }

    public string PublicContainerName { get; init; } = "public";
}