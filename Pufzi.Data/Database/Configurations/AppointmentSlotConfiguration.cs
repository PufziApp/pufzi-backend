using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class AppointmentSlotConfiguration
    : IEntityTypeConfiguration<AppointmentSlot>
{
    public void Configure(EntityTypeBuilder<AppointmentSlot> builder)
    {
        builder.ToTable("AppointmentSlots", table =>
        {
            table.HasCheckConstraint(
                "CK_AppointmentSlots_ValidTimeRange",
                "\"EndAt\" > \"StartAt\"");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StartAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.EndAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Appointment)
            .WithOne(x => x.Slot)
            .HasForeignKey<AppointmentSlot>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BusinessMembership)
            .WithMany(x => x.AppointmentSlots)
            .HasForeignKey(x => x.BusinessMembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AppointmentId)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.BusinessMembershipId,
            x.StartAt,
            x.EndAt
        });
    }
}