using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class PetImageConfiguration
    : IEntityTypeConfiguration<PetImage>
{
    public void Configure(EntityTypeBuilder<PetImage> builder)
    {
        builder.ToTable("PetImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BlobName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.PetId,
            x.SortOrder
        });

        builder.HasOne(x => x.Pet)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}