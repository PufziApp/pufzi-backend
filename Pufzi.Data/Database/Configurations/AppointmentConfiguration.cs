using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments", table =>
        {
            table.HasCheckConstraint(
                "CK_Appointments_ValidTimeRange",
                "\"EndAt\" > \"StartAt\"");

            table.HasCheckConstraint(
                "CK_Appointments_PositiveDuration",
                "\"DurationMinutes\" > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StartAt)
            .IsRequired();

        builder.Property(x => x.EndAt)
            .IsRequired();

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasPrecision(12, 2);

        builder.HasOne(x => x.Booking)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Pet)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Services)
            .WithOne(x => x.Appointment)
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.BookingId, x.StartAt });

        builder.HasIndex(x => new { x.PetId, x.StartAt });

        builder.HasIndex(x => new { x.StartAt, x.EndAt });
    }
}