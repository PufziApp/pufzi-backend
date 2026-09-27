using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class ServicePackageConfiguration
    : IEntityTypeConfiguration<ServicePackage>
{
    public void Configure(
        EntityTypeBuilder<ServicePackage> builder)
    {
        builder.ToTable("ServicePackages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Icon)
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.BusinessId);

        builder.HasOne(x => x.Business)
            .WithMany(x => x.ServicePackages)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}