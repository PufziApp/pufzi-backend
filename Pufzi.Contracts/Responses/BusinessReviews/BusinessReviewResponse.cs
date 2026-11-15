
namespace Pufzi.Contracts.Responses.BusinessReviews;

/// <summary>
/// Reprezintă informațiile unei recenzii, inclusiv datele autorului și răspunsul salonului.
/// </summary>
public class BusinessReviewResponse
{
    /// <summary>
    /// Identificatorul unic al recenziei.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificatorul salonului evaluat.
    /// </summary>
    public Guid BusinessId { get; set; }

    /// <summary>
    /// Identificatorul rezervării pentru care a fost publicată recenzia.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// Identificatorul clientului care a publicat recenzia.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Prenumele clientului care a publicat recenzia.
    /// </summary>
    public string UserFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Numele de familie al clientului care a publicat recenzia.
    /// </summary>
    public string UserLastName { get; set; } = string.Empty;

    /// <summary>
    /// Ratingul acordat salonului, între 1 și 5 stele.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Comentariul opțional publicat de client.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Răspunsul opțional publicat de salon.
    /// </summary>
    public string? BusinessReply { get; set; }

    /// <summary>
    /// Identificatorul utilizatorului care a publicat răspunsul salonului.
    /// </summary>
    public Guid? RepliedByUserId { get; set; }

    /// <summary>
    /// Data și ora publicării răspunsului salonului, în format UTC.
    /// </summary>
    public DateTime? RepliedAt { get; set; }

    /// <summary>
    /// Data și ora publicării recenziei, în format UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data și ora ultimei modificări a recenziei sau a răspunsului salonului, în format UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
