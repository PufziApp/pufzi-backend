using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class ServicePackageEmployeeConfiguration
    : IEntityTypeConfiguration<ServicePackageEmployee>
{
    public void Configure(
        EntityTypeBuilder<ServicePackageEmployee> builder)
    {
        builder.ToTable("ServicePackageEmployees");

        builder.HasKey(x => new
        {
            x.ServicePackageId,
            x.BusinessMembershipId
        });

        builder.HasOne(x => x.ServicePackage)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.ServicePackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BusinessMembership)
            .WithMany(x => x.ServicePackages)
            .HasForeignKey(x => x.BusinessMembershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}