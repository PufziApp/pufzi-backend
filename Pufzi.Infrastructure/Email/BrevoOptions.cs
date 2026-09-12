namespace Pufzi.Infrastructure.Email;

public class BrevoOptions
{
    public const string SectionName = "Brevo";

    public required string ApiKey { get; init; }

    public required string SenderEmail { get; init; }

    public required string SenderName { get; init; }
}