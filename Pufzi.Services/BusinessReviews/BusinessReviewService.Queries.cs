
using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.BusinessReviews;
using Pufzi.Contracts.Responses.BusinessReviews;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.BusinessReviews;

public partial class BusinessReviewService
{
    public async Task<PagedResponse<BusinessReviewResponse>> GetMyReviewsAsync(
        Guid userId,
        BusinessReviewQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Utilizatorul este invalid.");

        ValidateReviewQuery(request);

        var query = _dbContext.BusinessReviews
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        query = ApplyReviewFiltersAndSorting(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        var offset = ((long)request.Page - 1) * request.PageSize;

        var items = offset > int.MaxValue
            ? new List<BusinessReviewResponse>()
            : await query
                .Skip((int)offset)
                .Take(request.PageSize)
                .Select(x => new BusinessReviewResponse
                {
                    Id = x.Id,
                    BusinessId = x.BusinessId,
                    BookingId = x.BookingId,
                    UserId = x.UserId,
                    UserFirstName = x.User.FirstName,
                    UserLastName = x.User.LastName,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    BusinessReply = x.BusinessReply,
                    RepliedByUserId = x.RepliedByUserId,
                    RepliedAt = x.RepliedAt,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);

        return new PagedResponse<BusinessReviewResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResponse<PublicBusinessReviewResponse>> GetBusinessReviewsAsync(
        Guid? userId,
        Guid businessId,
        BusinessReviewQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("Salonul este invalid.");

        ValidateReviewQuery(request);

        await RequireBusinessReviewsReadAccessAsync(
            userId,
            businessId,
            cancellationToken);

        var query = _dbContext.BusinessReviews
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        query = ApplyReviewFiltersAndSorting(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        var offset = ((long)request.Page - 1) * request.PageSize;

        var items = offset > int.MaxValue
            ? new List<PublicBusinessReviewResponse>()
            : await query
                .Skip((int)offset)
                .Take(request.PageSize)
                .Select(x => new PublicBusinessReviewResponse
                {
                    Id = x.Id,
                    UserFirstName = x.User.FirstName,
                    UserLastNameInitial =
                        x.User.LastName.Length > 0
                            ? x.User.LastName.Substring(0, 1) + "."
                            : "",
                    Rating = x.Rating,
                    Comment = x.Comment,
                    BusinessReply = x.BusinessReply,
                    RepliedAt = x.RepliedAt,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);

        return new PagedResponse<PublicBusinessReviewResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<BusinessReviewSummaryResponse> GetBusinessReviewSummaryAsync(
        Guid? userId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("Salonul este invalid.");

        await RequireBusinessReviewsReadAccessAsync(
            userId,
            businessId,
            cancellationToken);

        var ratingCounts = await _dbContext.BusinessReviews
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId)
            .GroupBy(x => x.Rating)
            .Select(x => new
            {
                Rating = x.Key,
                Count = x.Count()
            })
            .ToListAsync(cancellationToken);

        var totalReviews = ratingCounts.Sum(x => x.Count);

        var averageRating = totalReviews == 0
            ? 0m
            : Math.Round(
                ratingCounts.Sum(
                    x => (decimal)x.Rating * x.Count)
                    / totalReviews,
                2,
                MidpointRounding.AwayFromZero);

        return new BusinessReviewSummaryResponse
        {
            BusinessId = businessId,
            AverageRating = averageRating,
            TotalReviews = totalReviews,

            FiveStarCount = ratingCounts
                .Where(x => x.Rating == 5)
                .Sum(x => x.Count),

            FourStarCount = ratingCounts
                .Where(x => x.Rating == 4)
                .Sum(x => x.Count),

            ThreeStarCount = ratingCounts
                .Where(x => x.Rating == 3)
                .Sum(x => x.Count),

            TwoStarCount = ratingCounts
                .Where(x => x.Rating == 2)
                .Sum(x => x.Count),

            OneStarCount = ratingCounts
                .Where(x => x.Rating == 1)
                .Sum(x => x.Count)
        };
    }

    private async Task RequireBusinessReviewsReadAccessAsync(
        Guid? userId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var business = await _dbContext.Businesses
            .AsNoTracking()
            .Where(x => x.Id == businessId)
            .Select(x => new
            {
                x.IsPublished
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (business is null)
            throw new KeyNotFoundException(
                "Salonul nu a fost găsit.");

        if (business.IsPublished)
            return;

        if (!userId.HasValue || userId.Value == Guid.Empty)
            throw new KeyNotFoundException(
                "Salonul nu a fost găsit.");

        var authorized = await _dbContext.BusinessMemberships
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.BusinessId == businessId &&
                    x.UserId == userId.Value &&
                    x.IsActive &&
                    x.RemovedAt == null &&
                    x.User.IsActive &&
                    (
                        x.Role == BusinessRole.Owner ||
                        x.Role == BusinessRole.Receptionist
                    ),
                cancellationToken);

        if (!authorized)
            throw new KeyNotFoundException(
                "Salonul nu a fost găsit.");
    }

    private static void ValidateReviewQuery(
        BusinessReviewQueryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Rating is < 1 or > 5)
            throw new ArgumentException(
                "Ratingul trebuie să fie între 1 și 5.");

        if (!Enum.IsDefined(request.SortBy))
            throw new ArgumentException(
                "Câmpul de sortare este invalid.");

        if (!Enum.IsDefined(request.SortDirection))
            throw new ArgumentException(
                "Direcția de sortare este invalidă.");

        if (request.Page < 1 || request.PageSize < 1 ||
            request.PageSize > 100)
        {
            throw new ArgumentException(
                "Parametrii de paginare sunt invalizi.");
        }
    }

    private static IQueryable<BusinessReview> ApplyReviewFiltersAndSorting(
        IQueryable<BusinessReview> query,
        BusinessReviewQueryRequest request)
    {
        if (request.Rating.HasValue)
        {
            var rating = request.Rating.Value;

            query = query.Where(x => x.Rating == rating);
        }

        if (request.HasBusinessReply.HasValue)
        {
            query = request.HasBusinessReply.Value
                ? query.Where(x => x.BusinessReply != null)
                : query.Where(x => x.BusinessReply == null);
        }

        return (request.SortBy, request.SortDirection) switch
        {
            (BusinessReviewSortBy.CreatedAt, SortDirection.Asc) =>
                query.OrderBy(x => x.CreatedAt)
                    .ThenBy(x => x.Id),

            (BusinessReviewSortBy.CreatedAt, SortDirection.Desc) =>
                query.OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id),

            (BusinessReviewSortBy.Rating, SortDirection.Asc) =>
                query.OrderBy(x => x.Rating)
                    .ThenByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id),

            (BusinessReviewSortBy.Rating, SortDirection.Desc) =>
                query.OrderByDescending(x => x.Rating)
                    .ThenByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id),

            (BusinessReviewSortBy.UpdatedAt, SortDirection.Asc) =>
                query.OrderBy(x => x.UpdatedAt)
                    .ThenBy(x => x.Id),

            (BusinessReviewSortBy.UpdatedAt, SortDirection.Desc) =>
                query.OrderByDescending(x => x.UpdatedAt)
                    .ThenByDescending(x => x.Id),

            _ => throw new ArgumentException(
                "Sortarea selectată este invalidă.")
        };
    }
}
