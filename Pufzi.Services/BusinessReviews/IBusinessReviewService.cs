
using Pufzi.Contracts.Common;
using Pufzi.Contracts.Requests.BusinessReviews;
using Pufzi.Contracts.Responses.BusinessReviews;

namespace Pufzi.Services.BusinessReviews;

public interface IBusinessReviewService
{
    Task<BusinessReviewResponse> CreateReviewAsync(
        Guid userId,
        CreateBusinessReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<BusinessReviewResponse> UpdateReviewAsync(
        Guid userId,
        Guid reviewId,
        UpdateBusinessReviewRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteReviewAsync(
        Guid userId,
        Guid reviewId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<BusinessReviewResponse>> GetMyReviewsAsync(
        Guid userId,
        BusinessReviewQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<PublicBusinessReviewResponse>> GetBusinessReviewsAsync(
        Guid? userId,
        Guid businessId,
        BusinessReviewQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<BusinessReviewSummaryResponse> GetBusinessReviewSummaryAsync(
        Guid? userId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<BusinessReviewResponse> ReplyToReviewAsync(
        Guid userId,
        Guid reviewId,
        BusinessReviewReplyRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteReviewReplyAsync(
        Guid userId,
        Guid reviewId,
        CancellationToken cancellationToken = default);
}
