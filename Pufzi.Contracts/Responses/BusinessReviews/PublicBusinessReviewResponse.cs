
namespace Pufzi.Contracts.Responses.BusinessReviews;

/// <summary>
/// Reprezintă informațiile unei recenzii care pot fi afișate
/// public, fără identificatori interni ai clienților sau rezervărilor.
/// </summary>
public class PublicBusinessReviewResponse
{
    /// <summary>
    /// Identificatorul unic al recenziei.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Prenumele autorului recenziei.
    /// </summary>
    public string UserFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Inițiala numelui de familie al autorului.
    /// </summary>
    public string UserLastNameInitial { get; set; } = string.Empty;

    /// <summary>
    /// Ratingul acordat salonului, între 1 și 5 stele.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Comentariul opțional al clientului.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Răspunsul opțional publicat de salon.
    /// </summary>
    public string? BusinessReply { get; set; }

    /// <summary>
    /// Data și ora publicării răspunsului salonului, în UTC.
    /// </summary>
    public DateTime? RepliedAt { get; set; }

    /// <summary>
    /// Data și ora publicării recenziei, în UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data și ora ultimei modificări, în UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
