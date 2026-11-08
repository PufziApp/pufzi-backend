namespace Pufzi.Data.Database.Entities;

public class AppointmentServiceVariant
{
    public Guid Id { get; set; }

    public Guid AppointmentServiceId { get; set; }

    public Guid ServicePackageItemVariantId { get; set; }

    public required string ItemName { get; set; }

    public required string VariantName { get; set; }

    public decimal? Price { get; set; }

    public bool IsStartingPrice { get; set; }

    public bool IsPriceOnRequest { get; set; }

    public int DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    public AppointmentService AppointmentService { get; set; } = null!;

    public ServicePackageItemVariant ServicePackageItemVariant { get; set; } =
        null!;
}