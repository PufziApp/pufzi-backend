using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Pufzi.Infrastructure.Email;

namespace Pufzi.Tests.Infrastructure.Email;

public class BrevoEmailServiceTests
{
    [Fact]
    public async Task SendAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        HttpMethod? capturedMethod = null;
        Uri? capturedUri = null;
        string? capturedApiKey = null;
        string? capturedContent = null;

        var handler = new TestHttpMessageHandler(
            async (request, cancellationToken) =>
            {
                capturedMethod = request.Method;
                capturedUri = request.RequestUri;

                if (request.Headers.TryGetValues(
                        "api-key",
                        out var apiKeyValues))
                {
                    capturedApiKey =
                        apiKeyValues.Single();
                }

                if (request.Content is not null)
                {
                    capturedContent =
                        await request.Content.ReadAsStringAsync(
                            cancellationToken);
                }

                return new HttpResponseMessage(
                    HttpStatusCode.Created);
            });

        var service = CreateService(handler);

        // Act
        await service.SendAsync(
            "client@example.com",
            "Test subject",
            "<h1>Hello</h1>");

        // Assert
        capturedMethod.Should()
            .Be(HttpMethod.Post);

        capturedUri.Should()
            .NotBeNull();

        capturedUri!.ToString().Should()
            .Be("https://api.brevo.com/v3/smtp/email");

        capturedApiKey.Should()
            .Be("test-api-key");

        capturedContent.Should()
            .NotBeNullOrWhiteSpace();

        using var document =
            JsonDocument.Parse(capturedContent!);

        var root =
            document.RootElement;

        root.GetProperty("subject")
            .GetString()
            .Should()
            .Be("Test subject");

        root.GetProperty("htmlContent")
            .GetString()
            .Should()
            .Be("<h1>Hello</h1>");

        var sender =
            root.GetProperty("sender");

        sender.GetProperty("name")
            .GetString()
            .Should()
            .Be("Pufzi");

        sender.GetProperty("email")
            .GetString()
            .Should()
            .Be("no-reply@pufzi.ro");

        var recipients =
            root.GetProperty("to");

        recipients.GetArrayLength()
            .Should()
            .Be(1);

        recipients[0]
            .GetProperty("email")
            .GetString()
            .Should()
            .Be("client@example.com");
    }

    [Fact]
    public async Task SendAsync_ShouldComplete_WhenBrevoReturnsOk()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(
            (_, _) =>
                Task.FromResult(
                    new HttpResponseMessage(
                        HttpStatusCode.OK)));

        var service =
            CreateService(handler);

        // Act
        var act = async () =>
            await service.SendAsync(
                "client@example.com",
                "Subject",
                "<p>Hello</p>");

        // Assert
        await act.Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_ShouldComplete_WhenBrevoReturnsCreated()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(
            (_, _) =>
                Task.FromResult(
                    new HttpResponseMessage(
                        HttpStatusCode.Created)));

        var service =
            CreateService(handler);

        // Act
        var act = async () =>
            await service.SendAsync(
                "client@example.com",
                "Subject",
                "<p>Hello</p>");

        // Assert
        await act.Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenBrevoReturnsBadRequest()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(
            (_, _) =>
            {
                var response =
                    new HttpResponseMessage(
                        HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(
                            """{"message":"Invalid email"}""")
                    };

                return Task.FromResult(response);
            });

        var service =
            CreateService(handler);

        // Act
        var act = async () =>
            await service.SendAsync(
                "invalid@example.com",
                "Subject",
                "<p>Hello</p>");

        // Assert
        var exception =
            await act.Should()
                .ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should()
            .Contain("Brevo email could not be sent.");

        exception.Which.Message.Should()
            .Contain("400");

        exception.Which.Message.Should()
            .Contain("Invalid email");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenBrevoReturnsUnauthorized()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(
            (_, _) =>
            {
                var response =
                    new HttpResponseMessage(
                        HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(
                            """{"message":"Invalid API key"}""")
                    };

                return Task.FromResult(response);
            });

        var service =
            CreateService(handler);

        // Act
        var act = async () =>
            await service.SendAsync(
                "client@example.com",
                "Subject",
                "<p>Hello</p>");

        // Assert
        var exception =
            await act.Should()
                .ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should()
            .Contain("401");

        exception.Which.Message.Should()
            .Contain("Invalid API key");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenBrevoReturnsInternalServerError()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(
            (_, _) =>
            {
                var response =
                    new HttpResponseMessage(
                        HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(
                            "Brevo internal error")
                    };

                return Task.FromResult(response);
            });

        var service =
            CreateService(handler);

        // Act
        var act = async () =>
            await service.SendAsync(
                "client@example.com",
                "Subject",
                "<p>Hello</p>");

        // Assert
        var exception =
            await act.Should()
                .ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should()
            .Contain("500");

        exception.Which.Message.Should()
            .Contain("Brevo internal error");
    }

    [Fact]
    public async Task SendAsync_ShouldPassCancelableToken_ToHttpClient()
    {
        // Arrange
        CancellationToken capturedToken = default;

        var handler = new TestHttpMessageHandler(
            (_, cancellationToken) =>
            {
                capturedToken = cancellationToken;

                return Task.FromResult(
                    new HttpResponseMessage(
                        HttpStatusCode.Created));
            });

        var service =
            CreateService(handler);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        // Act
        await service.SendAsync(
            "client@example.com",
            "Subject",
            "<p>Hello</p>",
            cancellationTokenSource.Token);

        // Assert
        capturedToken.CanBeCanceled
            .Should()
            .BeTrue();

        capturedToken.IsCancellationRequested
            .Should()
            .BeFalse();
    }

    private static BrevoEmailService CreateService(
        HttpMessageHandler handler)
    {
        var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri("https://api.brevo.com/")
            };

        var options =
            Options.Create(
                new BrevoOptions
                {
                    ApiKey = "test-api-key",
                    SenderEmail = "no-reply@pufzi.ro",
                    SenderName = "Pufzi"
                });

        return new BrevoEmailService(
            httpClient,
            options);
    }

    private sealed class TestHttpMessageHandler
        : HttpMessageHandler
    {
        private readonly Func<
            HttpRequestMessage,
            CancellationToken,
            Task<HttpResponseMessage>> _handler;

        public TestHttpMessageHandler(
            Func<
                HttpRequestMessage,
                CancellationToken,
                Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            return _handler(
                request,
                cancellationToken);
        }
    }
}