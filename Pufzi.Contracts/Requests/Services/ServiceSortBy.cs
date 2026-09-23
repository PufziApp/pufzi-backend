namespace Pufzi.Contracts.Requests.Services;

/// <summary>
/// Câmpurile disponibile pentru sortarea serviciilor.
/// </summary>
public enum ServiceSortBy
{
    /// <summary>
    /// Sortează după ordinea de afișare configurată de salon.
    /// </summary>
    SortOrder = 1,

    /// <summary>
    /// Sortează după numele serviciului.
    /// </summary>
    Name = 2,

    /// <summary>
    /// Sortează după prețul serviciului.
    /// </summary>
    Price = 3,

    /// <summary>
    /// Sortează după data creării serviciului.
    /// </summary>
    CreatedAt = 4,

    /// <summary>
    /// Sortează după data ultimei modificări.
    /// </summary>
    UpdatedAt = 5
}