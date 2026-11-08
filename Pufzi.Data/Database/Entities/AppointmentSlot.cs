namespace Pufzi.Data.Database.Entities;

public class AppointmentSlot
{
    public Guid Id { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid BusinessMembershipId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Appointment Appointment { get; set; } = null!;

    public BusinessMembership BusinessMembership { get; set; } = null!;
}