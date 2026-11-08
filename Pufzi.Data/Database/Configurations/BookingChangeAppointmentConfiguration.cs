using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class BookingChangeAppointmentConfiguration
    : IEntityTypeConfiguration<BookingChangeAppointment>
{
    public void Configure(EntityTypeBuilder<BookingChangeAppointment> builder)
    {
        builder.ToTable("BookingChangeAppointments", table =>
        {
            table.HasCheckConstraint(
                "CK_BookingChangeAppointments_ValidTimeRange",
                "\"ProposedEndAt\" > \"ProposedStartAt\"");
        });

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.BookingChangeRequest)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.BookingChangeRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Appointment)
            .WithMany(x => x.ChangeAppointments)
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.BookingChangeRequestId,
            x.AppointmentId
        }).IsUnique();
    }
}