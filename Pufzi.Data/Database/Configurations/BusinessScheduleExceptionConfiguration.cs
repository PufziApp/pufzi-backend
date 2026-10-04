using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class BusinessScheduleExceptionConfiguration
    : IEntityTypeConfiguration<BusinessScheduleException>
{
    public void Configure(
        EntityTypeBuilder<BusinessScheduleException> builder)
    {
        builder.ToTable("BusinessScheduleExceptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.IsClosed)
            .IsRequired();

        builder.Property(x => x.OpenTime);

        builder.Property(x => x.CloseTime);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.HasIndex(x => new
        {
            x.BusinessId,
            x.Date
        })
        .IsUnique();

        builder.HasOne(x => x.Business)
            .WithMany(x => x.ScheduleExceptions)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}