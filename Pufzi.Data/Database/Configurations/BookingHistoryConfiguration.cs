using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class BookingHistoryConfiguration
    : IEntityTypeConfiguration<BookingHistory>
{
    public void Configure(EntityTypeBuilder<BookingHistory> builder)
    {
        builder.ToTable("BookingHistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PreviousStatus)
            .HasConversion<int?>();

        builder.Property(x => x.NewStatus)
            .HasConversion<int?>();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.HasOne(x => x.Booking)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PerformedByUser)
            .WithMany(x => x.BookingHistoryEntries)
            .HasForeignKey(x => x.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BookingId, x.CreatedAt });
    }
}