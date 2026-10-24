
namespace Pufzi.Contracts.Responses.Pets;

public class PetImageResponse
{
    public Guid Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}
