using Pufzi.Contracts.Enums;

namespace Pufzi.Contracts.Responses.ServicePackages;

/// <summary>
/// Reprezintă un membru al echipei asignat unui pachet de servicii.
/// </summary>
public class ServicePackageEmployeeResponse
{
    /// <summary>
    /// ID-ul BusinessMembership-ului angajatului.
    /// </summary>
    public Guid BusinessMembershipId { get; init; }

    /// <summary>
    /// ID-ul utilizatorului asociat membrului echipei.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Prenumele membrului echipei.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Numele de familie al membrului echipei.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Rolul membrului în cadrul salonului.
    /// </summary>
    public BusinessRoleRequest Role { get; init; }

    /// <summary>
    /// Titlul profesional al membrului.
    /// Poate fi null.
    /// </summary>
    public string? JobTitle { get; init; }

    /// <summary>
    /// URL-ul public al imaginii de profil.
    /// Poate fi null.
    /// </summary>
    public string? ProfileImageUrl { get; init; }
}