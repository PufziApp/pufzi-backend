using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class ServicePriceVariantConfiguration
    : IEntityTypeConfiguration<ServicePriceVariant>
{
    public void Configure(
        EntityTypeBuilder<ServicePriceVariant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.MinWeightKg)
            .HasPrecision(6, 2);

        builder.Property(x => x.MaxWeightKg)
            .HasPrecision(6, 2);

        builder.Property(x => x.Price)
            .HasPrecision(10, 2);

        builder.Property(x => x.IsStartingPrice)
            .IsRequired();

        builder.Property(x => x.IsPriceOnRequest)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasOne(x => x.ServiceOption)
            .WithMany(x => x.PriceVariants)
            .HasForeignKey(x => x.ServiceOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ServiceOptionId);
    }
}