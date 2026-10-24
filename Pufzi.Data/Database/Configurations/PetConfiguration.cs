using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;
using Pufzi.Data.Database.Enums;

namespace Pufzi.Data.Database.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets", table =>
        {
            table.HasCheckConstraint(
                "CK_Pets_WeightKg",
                "\"WeightKg\" IS NULL OR " +
                "(\"WeightKg\" > 0 AND \"WeightKg\" <= 99999)");

            table.HasCheckConstraint(
                "CK_Pets_HeightCm",
                "\"HeightCm\" IS NULL OR " +
                "(\"HeightCm\" > 0 AND \"HeightCm\" <= 99999)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Sex)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.WeightKg)
            .HasPrecision(7, 2);

        builder.Property(x => x.HeightCm)
            .HasPrecision(7, 2);

        builder.Property(x => x.Color)
            .HasMaxLength(100);

        builder.Property(x => x.MicrochipStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(PetAnswer.Unknown)
            .IsRequired();

        builder.Property(x => x.MicrochipNumber)
            .HasMaxLength(30);

        builder.Property(x => x.SterilizationStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(PetAnswer.Unknown)
            .IsRequired();

        builder.Property(x => x.ImportantInformationStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(PetAnswer.Unknown)
            .IsRequired();

        builder.Property(x => x.ImportantInformation)
            .HasMaxLength(2000);

        builder.Property(x => x.ProfileImageBlobName)
            .HasMaxLength(500);

        builder.Property(x => x.IsArchived)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IsArchived,
            x.CreatedAt
        });

        builder.HasOne(x => x.User)
            .WithMany(x => x.Pets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AnimalSpecies)
            .WithMany(x => x.Pets)
            .HasForeignKey(x => x.AnimalSpeciesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AnimalBreed)
            .WithMany(x => x.Pets)
            .HasForeignKey(x => x.AnimalBreedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Pet)
            .HasForeignKey(x => x.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}