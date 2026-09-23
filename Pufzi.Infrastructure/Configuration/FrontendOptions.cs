namespace Pufzi.Infrastructure.Configuration;

public class FrontendOptions
{
    public const string SectionName = "Frontend";

    public required string WebUrl { get; set; }

    public required string MobileUrl { get; set; }
}