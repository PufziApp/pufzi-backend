
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class NotificationPreferenceConfiguration
    : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(
        EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences", table =>
        {
            table.HasCheckConstraint(
                "CK_NotificationPreferences_Language",
                "\"PreferredLanguage\" IN ('ro', 'en', 'hu')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PreferredLanguage)
            .HasMaxLength(5)
            .HasDefaultValue("ro")
            .IsRequired();

        builder.Property(x => x.PushEnabled)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.BookingNotificationsEnabled)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.ReminderNotificationsEnabled)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.ReviewNotificationsEnabled)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.TeamNotificationsEnabled)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.LeaveNotificationsEnabled)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}
