using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Pufzi.Infrastructure.Email;

public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly BrevoOptions _options;

    public BrevoEmailService(
        HttpClient httpClient,
        IOptions<BrevoOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlContent,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            sender = new
            {
                name = _options.SenderName,
                email = _options.SenderEmail
            },
            to = new[]
            {
                new
                {
                    email = to
                }
            },
            subject,
            htmlContent
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "v3/smtp/email");

        httpRequest.Headers.Add(
            "api-key",
            _options.ApiKey);

        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(
            httpRequest,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content
                .ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Brevo email could not be sent. " +
                $"Status: {(int)response.StatusCode}. " +
                $"Response: {errorContent}");
        }
    }
}