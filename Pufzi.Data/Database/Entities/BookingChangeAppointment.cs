namespace Pufzi.Data.Database.Entities;

public class BookingChangeAppointment
{
    public Guid Id { get; set; }

    public Guid BookingChangeRequestId { get; set; }

    public Guid AppointmentId { get; set; }

    public DateTime ProposedStartAt { get; set; }

    public DateTime ProposedEndAt { get; set; }

    public BookingChangeRequest BookingChangeRequest { get; set; } = null!;

    public Appointment Appointment { get; set; } = null!;
}