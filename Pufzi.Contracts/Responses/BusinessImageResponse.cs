namespace Pufzi.Contracts.Responses.Businesses;

public class BusinessImageResponse
{
    public Guid Id { get; init; }

    public required string Url { get; init; }

    public int SortOrder { get; init; }
}