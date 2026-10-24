namespace Pufzi.Contracts.Requests.Salons;

public class SalonSearchRequest
{
    public string? Search { get; set; }

    public string? City { get; set; }

    public string? County { get; set; }

    public string? CountryCode { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}