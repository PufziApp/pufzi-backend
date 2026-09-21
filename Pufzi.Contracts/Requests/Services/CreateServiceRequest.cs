using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Services;

public class CreateServiceRequest
{
    [Required]
    [MaxLength(150)]
    public required string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    [Required]
    [MinLength(1)]
    public required ICollection<CreateServiceOptionRequest>
        Options
    { get; set; }
}