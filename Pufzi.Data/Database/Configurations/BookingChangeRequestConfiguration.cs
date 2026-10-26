using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class BookingChangeRequestConfiguration
    : IEntityTypeConfiguration<BookingChangeRequest>
{
    public void Configure(EntityTypeBuilder<BookingChangeRequest> builder)
    {
        builder.ToTable("BookingChangeRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(2000);

        builder.Property(x => x.ReviewNote)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Booking)
            .WithMany(x => x.ChangeRequests)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestedByUser)
            .WithMany(x => x.RequestedBookingChanges)
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany(x => x.ReviewedBookingChanges)
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BookingId, x.Status });

        builder.HasIndex(x => x.ExpiresAt);

        builder.HasIndex(x => x.BookingId)
            .IsUnique()
            .HasFilter("\"Status\" = 1");
    }
}