namespace Pufzi.Data.Database.Entities;

public class Appointment
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid PetId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public int DurationMinutes { get; set; }

    public decimal? TotalPrice { get; set; }

    public bool HasPriceOnRequest { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Booking Booking { get; set; } = null!;

    public Pet Pet { get; set; } = null!;

    public AppointmentSlot? Slot { get; set; }

    public ICollection<AppointmentService> Services { get; set; } = [];

    public ICollection<BookingChangeAppointment> ChangeAppointments { get; set; } = [];

}