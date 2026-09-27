namespace Pufzi.Data.Database.Entities;

public class ServicePackageItem
{
    public Guid Id { get; set; }

    public Guid ServicePackageId { get; set; }

    public required string Name { get; set; }

    public int SortOrder { get; set; }

    public ServicePackage ServicePackage { get; set; } = null!;

    public ICollection<ServicePackageItemOption> Options { get; set; } = [];
}