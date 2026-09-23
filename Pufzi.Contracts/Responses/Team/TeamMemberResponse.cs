using Pufzi.Contracts.Enums;

namespace Pufzi.Contracts.Responses.Team;

/// <summary>
/// Reprezintă un membru al echipei unui salon.
/// </summary>
public class TeamMemberResponse
{
    /// <summary>
    /// Identificatorul unic al utilizatorului.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Prenumele membrului echipei.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Numele de familie al membrului echipei.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Adresa de email a membrului echipei.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Numărul de telefon al membrului.
    /// Este null dacă nu a fost completat.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// URL-ul public al imaginii de profil.
    /// Este null dacă utilizatorul nu are imagine de profil.
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Rolul membrului în cadrul salonului.
    /// </summary>
    public BusinessRoleRequest Role { get; set; }

    /// <summary>
    /// Titlul profesional al membrului în cadrul salonului.
    /// Este null dacă nu a fost completat.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Descrierea profesională a membrului.
    /// Este null dacă nu a fost completată.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Indică dacă membrul este activ în cadrul salonului.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Data și ora UTC la care utilizatorul a fost adăugat în echipă.
    /// </summary>
    public DateTime JoinedAt { get; set; }
}