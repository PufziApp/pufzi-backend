
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pufzi.Contracts.Requests.BusinessReviews;
using Pufzi.Contracts.Responses.BusinessReviews;
using Pufzi.Data.Database;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Services.BusinessReviews;

public partial class BusinessReviewService : IBusinessReviewService
{
    private readonly PufziDbContext _dbContext;

    public BusinessReviewService(PufziDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BusinessReviewResponse> CreateReviewAsync(
        Guid userId,
        CreateBusinessReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId == Guid.Empty || request.BookingId == Guid.Empty)
            throw new ArgumentException(
                "Utilizatorul sau rezervarea este invalidă.");

        ValidateRating(request.Rating);

        var comment = NormalizeOptionalText(request.Comment, 2000);

        var booking = await _dbContext.Bookings
            .AsNoTracking()
            .Where(x => x.Id == request.BookingId)
            .Select(x => new
            {
                x.Id,
                x.BusinessId,
                x.UserId,
                x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (booking is null)
            throw new KeyNotFoundException(
                "Rezervarea nu a fost găsită.");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException(
                "Nu poți evalua rezervarea altui utilizator.");

        if (booking.Status != BookingStatus.Completed)
            throw new InvalidOperationException(
                "Poți lăsa o recenzie doar pentru o rezervare finalizată.");

        var alreadyReviewed = await _dbContext.BusinessReviews
            .AsNoTracking()
            .AnyAsync(
                x => x.BookingId == booking.Id,
                cancellationToken);

        if (alreadyReviewed)
            throw new InvalidOperationException(
                "Această rezervare are deja o recenzie.");

        var nowUtc = DateTime.UtcNow;

        var review = new BusinessReview
        {
            Id = Guid.NewGuid(),
            BusinessId = booking.BusinessId,
            BookingId = booking.Id,
            UserId = userId,
            Rating = request.Rating,
            Comment = comment,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };

        _dbContext.BusinessReviews.Add(review);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_BusinessReviews_BookingId"
            })
        {
            throw new InvalidOperationException(
                "Această rezervare are deja o recenzie.");
        }

        return await GetReviewResponseAsync(
            review.Id,
            cancellationToken);
    }

    public async Task<BusinessReviewResponse> UpdateReviewAsync(
        Guid userId,
        Guid reviewId,
        UpdateBusinessReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId == Guid.Empty || reviewId == Guid.Empty)
            throw new ArgumentException(
                "Utilizatorul sau recenzia este invalidă.");

        ValidateRating(request.Rating);

        var comment = NormalizeOptionalText(request.Comment, 2000);

        var review = await _dbContext.BusinessReviews
            .FirstOrDefaultAsync(
                x => x.Id == reviewId,
                cancellationToken);

        if (review is null)
            throw new KeyNotFoundException(
                "Recenzia nu a fost găsită.");

        if (review.UserId != userId)
            throw new UnauthorizedAccessException(
                "Nu ai permisiunea să modifici această recenzie.");

        review.Rating = request.Rating;
        review.Comment = comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetReviewResponseAsync(
            review.Id,
            cancellationToken);
    }

    public async Task DeleteReviewAsync(
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

        if (review.UserId != userId)
            throw new UnauthorizedAccessException(
                "Nu ai permisiunea să ștergi această recenzie.");

        _dbContext.BusinessReviews.Remove(review);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateRating(int rating)
    {
        if (rating is < 1 or > 5)
            throw new ArgumentException(
                "Ratingul trebuie să fie între 1 și 5.");
    }

    private static string? NormalizeOptionalText(
        string? value,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new ArgumentException(
                $"Textul nu poate depăși {maxLength} de caractere.");

        return normalized;
    }

    private async Task<BusinessReviewResponse> GetReviewResponseAsync(
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        var review = await _dbContext.BusinessReviews
            .AsNoTracking()
            .Where(x => x.Id == reviewId)
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
            .FirstOrDefaultAsync(cancellationToken);

        return review ?? throw new KeyNotFoundException(
            "Recenzia nu a fost găsită.");
    }
}
