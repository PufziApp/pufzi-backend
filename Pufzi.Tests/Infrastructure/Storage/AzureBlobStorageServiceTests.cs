using FluentAssertions;
using Microsoft.Extensions.Options;
using Pufzi.Infrastructure.Storage;

namespace Pufzi.Tests.Infrastructure.Storage;

public class AzureBlobStorageServiceTests
{
    private const string ConnectionString =
        "DefaultEndpointsProtocol=http;" +
        "AccountName=devstoreaccount1;" +
        "AccountKey=" +
        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
        "BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";

    [Fact]
    public void Constructor_ShouldCreateService_WithValidConfiguration()
    {
        var options =
            Options.Create(
                new AzureBlobStorageOptions
                {
                    ConnectionString =
                        ConnectionString,

                    PublicContainerName =
                        "public"
                });

        var act = () =>
            new AzureBlobStorageService(options);

        act.Should().NotThrow();
    }

    [Fact]
    public void GetPublicUrl_ShouldReturnCorrectBlobUrl()
    {
        var options =
            Options.Create(
                new AzureBlobStorageOptions
                {
                    ConnectionString =
                        ConnectionString,

                    PublicContainerName =
                        "public"
                });

        var service =
            new AzureBlobStorageService(options);

        var url =
            service.GetPublicUrl(
                "businesses/test/logo/image.jpg");

        url.Should()
            .Be(
                "http://127.0.0.1:10000/devstoreaccount1/public/businesses/test/logo/image.jpg");
    }
}