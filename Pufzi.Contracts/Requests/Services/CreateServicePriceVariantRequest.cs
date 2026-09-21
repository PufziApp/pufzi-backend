using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Services;

public class CreateServicePriceVariantRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Range(0, 9999)]
    public decimal? MinWeightKg { get; set; }

    [Range(0, 9999)]
    public decimal? MaxWeightKg { get; set; }

    [Range(0, 999999)]
    public decimal? Price { get; set; }

    public bool IsStartingPrice { get; set; }

    public bool IsPriceOnRequest { get; set; }

    [Range(1, 1440)]
    public int? DurationMinutes { get; set; }

    public int SortOrder { get; set; }
}