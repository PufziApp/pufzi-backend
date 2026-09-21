namespace Pufzi.Infrastructure.Email;

public interface IEmailService
{
    Task SendAsync(
        string to,
        string subject,
        string htmlContent,
        CancellationToken cancellationToken = default);
}