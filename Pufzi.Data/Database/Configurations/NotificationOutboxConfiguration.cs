
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class NotificationOutboxConfiguration
    : IEntityTypeConfiguration<NotificationOutbox>
{
    public void Configure(
        EntityTypeBuilder<NotificationOutbox> builder)
    {
        builder.ToTable("NotificationOutbox", table =>
        {
            table.HasCheckConstraint(
                "CK_NotificationOutbox_AttemptCount",
                "\"AttemptCount\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.AvailableAt)
            .IsRequired();

        builder.Property(x => x.AttemptCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.HasOne(x => x.Notification)
            .WithMany()
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.ProcessedAt,
            x.AvailableAt
        });

        builder.HasIndex(x => x.NotificationId)
            .IsUnique();
    }
}
