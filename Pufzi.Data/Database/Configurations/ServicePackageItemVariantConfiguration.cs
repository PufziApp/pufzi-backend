using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class ServicePackageItemVariantConfiguration
    : IEntityTypeConfiguration<ServicePackageItemVariant>
{
    public void Configure(
        EntityTypeBuilder<ServicePackageItemVariant> builder)
    {
        builder.ToTable("ServicePackageItemVariants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.MinWeightKg)
            .HasPrecision(8, 2);

        builder.Property(x => x.MaxWeightKg)
            .HasPrecision(8, 2);

        builder.Property(x => x.Price)
            .HasPrecision(10, 2);

        builder.Property(x => x.IsStartingPrice)
            .IsRequired();

        builder.Property(x => x.IsPriceOnRequest)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasIndex(x =>
            x.ServicePackageItemOptionId);

        builder.HasOne(x => x.ServicePackageItemOption)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.ServicePackageItemOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}