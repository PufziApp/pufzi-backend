namespace Pufzi.Contracts.Requests.ServicePackages;

/// <summary>
/// Câmpurile disponibile pentru sortarea pachetelor de servicii.
/// </summary>
public enum ServicePackageSortBy
{
    /// <summary>
    /// Sortează după ordinea configurată manual.
    /// </summary>
    SortOrder = 1,

    /// <summary>
    /// Sortează după numele pachetului.
    /// </summary>
    Name = 2,

    /// <summary>
    /// Sortează după cel mai mic preț disponibil.
    /// </summary>
    Price = 3,

    /// <summary>
    /// Sortează după data creării.
    /// </summary>
    CreatedAt = 4
}