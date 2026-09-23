using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class ServiceOptionConfiguration : IEntityTypeConfiguration<ServiceOption>
{
    public void Configure(EntityTypeBuilder<ServiceOption> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DurationMinutes);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasOne(x => x.Service)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AnimalSpecies)
            .WithMany(x => x.ServiceOptions)
            .HasForeignKey(x => x.AnimalSpeciesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.ServiceId,
            x.AnimalSpeciesId
        })
        .IsUnique();
    }
}