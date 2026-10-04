using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class EmployeeWorkingHourConfiguration
    : IEntityTypeConfiguration<EmployeeWorkingHour>
{
    public void Configure(
        EntityTypeBuilder<EmployeeWorkingHour> builder)
    {
        builder.ToTable("EmployeeWorkingHours");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayOfWeek)
            .IsRequired();

        builder.Property(x => x.IsWorking)
            .IsRequired();

        builder.Property(x => x.StartTime);

        builder.Property(x => x.EndTime);

        builder.HasIndex(x => new
        {
            x.BusinessMembershipId,
            x.DayOfWeek
        });

        builder.HasOne(x => x.BusinessMembership)
            .WithMany(x => x.WorkingHours)
            .HasForeignKey(x => x.BusinessMembershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}