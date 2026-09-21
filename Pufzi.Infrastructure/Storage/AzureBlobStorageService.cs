using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace Pufzi.Infrastructure.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _publicContainer;

    public AzureBlobStorageService(
        IOptions<AzureBlobStorageOptions> options)
    {
        var settings = options.Value;

        var blobServiceClient =
            new BlobServiceClient(settings.ConnectionString);

        _publicContainer =
            blobServiceClient.GetBlobContainerClient(
                settings.PublicContainerName);
    }

    public async Task<string> UploadPublicAsync(
        Stream stream,
        string blobName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await _publicContainer.CreateIfNotExistsAsync(
            PublicAccessType.Blob,
            cancellationToken: cancellationToken);

        var blobClient =
            _publicContainer.GetBlobClient(blobName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };

        await blobClient.UploadAsync(
            stream,
            uploadOptions,
            cancellationToken);

        return blobName;
    }

    public async Task DeletePublicAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var blobClient =
            _publicContainer.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(
            DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken);
    }

    public string GetPublicUrl(string blobName)
    {
        return _publicContainer
            .GetBlobClient(blobName)
            .Uri
            .ToString();
    }
}