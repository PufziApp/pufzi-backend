using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class BusinessWorkingHourConfiguration
    : IEntityTypeConfiguration<BusinessWorkingHour>
{
    public void Configure(
        EntityTypeBuilder<BusinessWorkingHour> builder)
    {
        builder.ToTable("BusinessWorkingHours");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayOfWeek)
            .IsRequired();

        builder.Property(x => x.IsOpen)
            .IsRequired();

        builder.Property(x => x.StartTime);

        builder.Property(x => x.EndTime);

        builder.HasIndex(x => new
        {
            x.BusinessId,
            x.DayOfWeek
        });

        builder.HasOne(x => x.Business)
            .WithMany(x => x.WorkingHours)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}