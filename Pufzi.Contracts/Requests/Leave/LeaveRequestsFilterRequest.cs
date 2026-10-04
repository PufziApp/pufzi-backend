using Pufzi.Contracts.Common;
using Pufzi.Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Leave;

/// <summary>
/// Filtrele și opțiunile de paginare pentru lista cererilor de concediu.
/// </summary>
public class LeaveRequestsFilterRequest
{
    /// <summary>
    /// Numărul paginii solicitate.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Numărul maxim de rezultate returnate pe pagină.
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Text utilizat pentru căutarea unui membru după nume.
    /// Se utilizează în lista de cereri disponibilă proprietarului.
    /// </summary>
    [MaxLength(200)]
    public string? Search { get; set; }

    /// <summary>
    /// Filtrează cererile după status.
    /// </summary>
    public LeaveRequestStatusRequest? Status { get; set; }

    /// <summary>
    /// Filtrează cererile după tipul concediului sau al absenței.
    /// </summary>
    public LeaveTypeRequest? LeaveType { get; set; }

    /// <summary>
    /// Include cererile care se intersectează cu perioada începând de la această dată.
    /// </summary>
    public DateOnly? FromDate { get; set; }

    /// <summary>
    /// Include cererile care se intersectează cu perioada până la această dată.
    /// </summary>
    public DateOnly? ToDate { get; set; }

    /// <summary>
    /// Câmpul utilizat pentru sortare.
    /// Valorile acceptate vor fi validate de backend.
    /// </summary>
    [MaxLength(50)]
    public string? SortBy { get; set; }

    /// <summary>
    /// Direcția de sortare.
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
}