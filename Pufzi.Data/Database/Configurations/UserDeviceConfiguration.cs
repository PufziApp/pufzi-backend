
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class UserDeviceConfiguration
    : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("UserDevices", table =>
        {
            table.HasCheckConstraint(
                "CK_UserDevices_Platform",
                "\"Platform\" IN ('ios', 'android')");

            table.HasCheckConstraint(
                "CK_UserDevices_Language",
                "\"Language\" IN ('ro', 'en', 'hu')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FcmToken)
            .HasMaxLength(4096)
            .IsRequired();

        builder.Property(x => x.Platform)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Language)
            .HasMaxLength(5)
            .HasDefaultValue("ro")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FcmToken)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IsActive
        });
    }
}
