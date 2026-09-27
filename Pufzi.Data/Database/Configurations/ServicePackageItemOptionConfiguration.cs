using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class ServicePackageItemOptionConfiguration
    : IEntityTypeConfiguration<ServicePackageItemOption>
{
    public void Configure(
        EntityTypeBuilder<ServicePackageItemOption> builder)
    {
        builder.ToTable("ServicePackageItemOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ServicePackageItemId,
            x.AnimalSpeciesId
        })
        .IsUnique();

        builder.HasOne(x => x.ServicePackageItem)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.ServicePackageItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AnimalSpecies)
            .WithMany()
            .HasForeignKey(x => x.AnimalSpeciesId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}