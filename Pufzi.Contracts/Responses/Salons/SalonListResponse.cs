namespace Pufzi.Contracts.Responses.Salons;

/// <summary>
/// Răspunsul paginat pentru lista saloanelor publicate.
/// Conține saloanele din pagina curentă și informațiile
/// necesare pentru navigarea între pagini.
/// </summary>
public class SalonListResponse
{
    /// <summary>
    /// Lista saloanelor returnate pentru pagina solicitată.
    /// Fiecare element conține informațiile rezumative
    /// definite în SalonListItemResponse.
    /// </summary>
    public IReadOnlyCollection<SalonListItemResponse> Items { get; init; }
        = [];

    /// <summary>
    /// Numărul paginii returnate.
    /// Numerotarea începe de la 1.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Numărul de saloane solicitate pe pagină.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Numărul total de saloane care corespund filtrelor,
    /// înainte de aplicarea paginării.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Numărul total de pagini disponibile
    /// pentru filtrele și dimensiunea paginii solicitate.
    /// </summary>
    public int TotalPages { get; init; }
}