namespace Pufzi.Data.Database.Entities;

public class AppointmentService
{
    public Guid Id { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid ServicePackageId { get; set; }

    public required string PackageName { get; set; }

    public int DurationMinutes { get; set; }

    public decimal? TotalPrice { get; set; }

    public bool HasPriceOnRequest { get; set; }

    public int SortOrder { get; set; }

    public Appointment Appointment { get; set; } = null!;

    public ServicePackage ServicePackage { get; set; } = null!;

    public ICollection<AppointmentServiceVariant> Variants { get; set; } = [];
}