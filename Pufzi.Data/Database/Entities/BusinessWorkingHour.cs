namespace Pufzi.Data.Database.Entities;

public class BusinessWorkingHour
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public bool IsOpen { get; set; }

    public TimeOnly? OpenTime { get; set; }

    public TimeOnly? CloseTime { get; set; }

    public Business Business { get; set; } = null!;
}