namespace Pufzi.Contracts.Responses.Pets;

/// <summary>
/// Informațiile despre o fotografie din galeria unui animal.
/// </summary>
public class PetImageResponse
{
    /// <summary>
    /// Identificatorul unic al fotografiei.
    /// Se utilizează pentru identificarea și ștergerea
    /// fotografiei din galeria animalului.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Adresa URL a fotografiei.
    /// Frontend-ul utilizează această adresă
    /// pentru afișarea imaginii.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Poziția fotografiei în galerie.
    /// Poate fi utilizată pentru ordonarea imaginilor.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Data și ora la care fotografia a fost adăugată.
    /// Valoarea este de tip DateTime.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}