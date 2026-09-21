using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Services;

public class CreateServiceOptionRequest
{
    public Guid AnimalSpeciesId { get; set; }

    [Range(1, 1440)]
    public int? DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    [Required]
    [MinLength(1)]
    public required ICollection<CreateServicePriceVariantRequest>
        PriceVariants
    { get; set; }
}