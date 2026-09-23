using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class AnimalSpeciesConfiguration : IEntityTypeConfiguration<AnimalSpecies>
{
    public void Configure(EntityTypeBuilder<AnimalSpecies> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasData(
            new AnimalSpecies
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Code = "dog",
                IsActive = true,
                SortOrder = 1
            },
            new AnimalSpecies
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Code = "cat",
                IsActive = true,
                SortOrder = 2
            },
            new AnimalSpecies
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Code = "rabbit",
                IsActive = true,
                SortOrder = 3
            },
            new AnimalSpecies
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Code = "guinea_pig",
                IsActive = true,
                SortOrder = 4
            },
            new AnimalSpecies
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Code = "hamster",
                IsActive = true,
                SortOrder = 5
            });
    }
}