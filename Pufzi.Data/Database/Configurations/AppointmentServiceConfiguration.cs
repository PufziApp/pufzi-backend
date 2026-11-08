using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class AppointmentServiceConfiguration
    : IEntityTypeConfiguration<AppointmentService>
{
    public void Configure(EntityTypeBuilder<AppointmentService> builder)
    {
        builder.ToTable("AppointmentServices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PackageName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasPrecision(12, 2);

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.HasOne(x => x.Appointment)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ServicePackage)
            .WithMany(x => x.AppointmentServices)
            .HasForeignKey(x => x.ServicePackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Variants)
            .WithOne(x => x.AppointmentService)
            .HasForeignKey(x => x.AppointmentServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ServicePackageId);
    }
}