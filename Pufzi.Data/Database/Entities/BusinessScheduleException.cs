namespace Pufzi.Data.Database.Entities;

public class BusinessScheduleException
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public DateOnly Date { get; set; }

    public bool IsClosed { get; set; }

    public TimeOnly? OpenTime { get; set; }

    public TimeOnly? CloseTime { get; set; }

    public string? Reason { get; set; }

    public Business Business { get; set; } = null!;
}