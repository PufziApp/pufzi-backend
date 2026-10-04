namespace Pufzi.Contracts.Enums;

/// <summary>
/// Statusurile posibile ale unei cereri de concediu sau absență.
/// </summary>
public enum LeaveRequestStatusRequest
{
    /// <summary>
    /// Cererea a fost trimisă și așteaptă procesarea proprietarului.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Cererea a fost aprobată.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Cererea a fost respinsă.
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Cererea a fost anulată.
    /// </summary>
    Cancelled = 4
}