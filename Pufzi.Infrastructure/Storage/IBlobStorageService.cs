namespace Pufzi.Infrastructure.Storage;

public interface IBlobStorageService
{
    Task<string> UploadPublicAsync(
        Stream stream,
        string blobName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeletePublicAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    string GetPublicUrl(string blobName);
}