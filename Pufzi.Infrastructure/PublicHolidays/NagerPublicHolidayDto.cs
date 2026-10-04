using System.Text.Json.Serialization;

namespace Pufzi.Infrastructure.PublicHolidays;

internal class NagerPublicHolidayDto
{
    [JsonPropertyName("date")]
    public DateOnly Date { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}