using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class EmployeeLeaveBalanceConfiguration
    : IEntityTypeConfiguration<EmployeeLeaveBalance>
{
    public void Configure(
        EntityTypeBuilder<EmployeeLeaveBalance> builder)
    {
        builder.ToTable("EmployeeLeaveBalances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.TotalDays)
            .IsRequired();

        builder.Property(x => x.UsedDays)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.BusinessMembershipId,
            x.Year
        })
        .IsUnique();

        builder.HasOne(x => x.BusinessMembership)
            .WithMany(x => x.LeaveBalances)
            .HasForeignKey(x => x.BusinessMembershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}