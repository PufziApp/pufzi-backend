using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class AppointmentServiceVariantConfiguration
    : IEntityTypeConfiguration<AppointmentServiceVariant>
{
    public void Configure(EntityTypeBuilder<AppointmentServiceVariant> builder)
    {
        builder.ToTable("AppointmentServiceVariants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.VariantName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(12, 2);

        builder.HasOne(x => x.AppointmentService)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.AppointmentServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ServicePackageItemVariant)
            .WithMany(x => x.AppointmentServiceVariants)
            .HasForeignKey(x => x.ServicePackageItemVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ServicePackageItemVariantId);
    }
}