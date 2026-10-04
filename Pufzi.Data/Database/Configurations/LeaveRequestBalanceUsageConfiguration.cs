using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class LeaveRequestBalanceUsageConfiguration
    : IEntityTypeConfiguration<LeaveRequestBalanceUsage>
{
    public void Configure(
        EntityTypeBuilder<LeaveRequestBalanceUsage> builder)
    {
        builder.ToTable("LeaveRequestBalanceUsages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Days)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.LeaveRequestId,
            x.Year
        })
        .IsUnique();

        builder.HasOne(x => x.LeaveRequest)
            .WithMany(x => x.BalanceUsages)
            .HasForeignKey(x => x.LeaveRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}