using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class ServicePackageAnimalSpeciesConfiguration
    : IEntityTypeConfiguration<ServicePackageAnimalSpecies>
{
    public void Configure(
        EntityTypeBuilder<ServicePackageAnimalSpecies> builder)
    {
        builder.ToTable("ServicePackageAnimalSpecies");

        builder.HasKey(x => new
        {
            x.ServicePackageId,
            x.AnimalSpeciesId
        });

        builder.HasOne(x => x.ServicePackage)
            .WithMany(x => x.AnimalSpecies)
            .HasForeignKey(x => x.ServicePackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AnimalSpecies)
            .WithMany()
            .HasForeignKey(x => x.AnimalSpeciesId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}