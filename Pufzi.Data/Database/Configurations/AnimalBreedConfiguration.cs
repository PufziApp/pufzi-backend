using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class AnimalBreedConfiguration
    : IEntityTypeConfiguration<AnimalBreed>
{
    public void Configure(EntityTypeBuilder<AnimalBreed> builder)
    {
        builder.ToTable("AnimalBreeds");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.AnimalSpeciesId,
            x.Code
        }).IsUnique();

        builder.HasOne(x => x.AnimalSpecies)
            .WithMany(x => x.Breeds)
            .HasForeignKey(x => x.AnimalSpeciesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(AnimalBreedSeed.GetBreeds());
    }
}