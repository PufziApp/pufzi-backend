using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class ServicePackageItemConfiguration
    : IEntityTypeConfiguration<ServicePackageItem>
{
    public void Configure(
        EntityTypeBuilder<ServicePackageItem> builder)
    {
        builder.ToTable("ServicePackageItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasIndex(x => x.ServicePackageId);

        builder.HasOne(x => x.ServicePackage)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ServicePackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}