namespace Pufzi.Contracts.Responses.Salons;

public class SalonListResponse
{
    public IReadOnlyCollection<SalonListItemResponse> Items { get; init; }
        = [];

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}