using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Services;

/// <summary>
/// Datele necesare pentru modificarea unui serviciu existent.
/// </summary>
public class UpdateServiceRequest
{
    /// <summary>
    /// Numele serviciului. Obligatoriu.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public required string Name { get; set; }

    /// <summary>
    /// Descrierea serviciului. Opțională.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Ordinea de afișare a serviciului în lista salonului.
    /// Valorile mai mici sunt afișate primele.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Configurările serviciului pentru speciile de animale acceptate.
    /// Lista trimisă înlocuiește configurațiile existente ale serviciului.
    /// Este necesară cel puțin o opțiune.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required ICollection<CreateServiceOptionRequest>
        Options
    { get; set; }
}