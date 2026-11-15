
using Microsoft.EntityFrameworkCore;
using Pufzi.Contracts.Requests.BusinessReviews;
using Pufzi.Contracts.Responses.BusinessReviews;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.BusinessReviews;

public partial class BusinessReviewService
{
    public async Task<BusinessReviewResponse> ReplyToReviewAsync(
        Guid userId,
        Guid reviewId,
        BusinessReviewReplyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId == Guid.Empty || reviewId == Guid.Empty)
            throw new ArgumentException(
                "Utilizatorul sau recenzia este invalidă.");

        if (string.IsNullOrWhiteSpace(request.Reply))
            throw new ArgumentException(
                "Răspunsul salonului este obligatoriu.");

        var reply = request.Reply.Trim();

        if (reply.Length > 2000)
            throw new ArgumentException(
                "Răspunsul nu poate depăși 2000 de caractere.");

        var review = await _dbContext.BusinessReviews
            .FirstOrDefaultAsync(
                x => x.Id == reviewId,
                cancellationToken);

        if (review is null)
            throw new KeyNotFoundException(
                "Recenzia nu a fost găsită.");

        await RequireBusinessReviewManagerAsync(
            userId,
            review.BusinessId,
            cancellationToken);

        var nowUtc = DateTime.UtcNow;

        review.BusinessReply = reply;
        review.RepliedByUserId = userId;
        review.RepliedAt = nowUtc;
        review.UpdatedAt = nowUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetReviewResponseAsync(
            review.Id,
            cancellationToken);
    }

    public async Task DeleteReviewReplyAsync(
        Guid userId,
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || reviewId == Guid.Empty)
            throw new ArgumentException(
                "Utilizatorul sau recenzia este invalidă.");

        var review = await _dbContext.BusinessReviews
            .FirstOrDefaultAsync(
                x => x.Id == reviewId,
                cancellationToken);

        if (review is null)
            throw new KeyNotFoundException(
                "Recenzia nu a fost găsită.");

        await RequireBusinessReviewManagerAsync(
            userId,
            review.BusinessId,
            cancellationToken);

        if (review.BusinessReply is null)
            throw new InvalidOperationException(
                "Această recenzie nu are un răspuns.");

        review.BusinessReply = null;
        review.RepliedByUserId = null;
        review.RepliedAt = null;
        review.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RequireBusinessReviewManagerAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var authorized = await _dbContext.BusinessMemberships
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.BusinessId == businessId &&
                    x.UserId == userId &&
                    x.IsActive &&
                    x.RemovedAt == null &&
                    x.User.IsActive &&
                    (
                        x.Role == BusinessRole.Owner ||
                        x.Role == BusinessRole.Receptionist
                    ),
                cancellationToken);

        if (!authorized)
            throw new UnauthorizedAccessException(
                "Nu ai permisiunea să gestionezi răspunsurile salonului.");
    }
}
