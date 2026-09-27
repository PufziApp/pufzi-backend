namespace Pufzi.Data.Database.Entities;

public class ServicePackageEmployee
{
    public Guid ServicePackageId { get; set; }

    public Guid BusinessMembershipId { get; set; }

    public ServicePackage ServicePackage { get; set; } = null!;

    public BusinessMembership BusinessMembership { get; set; } = null!;
}