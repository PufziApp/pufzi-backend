using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public class BusinessReviewConfiguration : IEntityTypeConfiguration<BusinessReview>
{
    public void Configure(EntityTypeBuilder<BusinessReview> builder)
    {
        builder.ToTable("BusinessReviews", table =>
        {
            table.HasCheckConstraint(
                "CK_BusinessReviews_Rating",
                "\"Rating\" >= 1 AND \"Rating\" <= 5");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(2000);

        builder.Property(x => x.BusinessReply)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.Business)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Booking)
            .WithOne(x => x.Review)
            .HasForeignKey<BusinessReview>(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.BusinessReviews)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RepliedByUser)
            .WithMany(x => x.BusinessReviewReplies)
            .HasForeignKey(x => x.RepliedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BookingId)
            .IsUnique();

        builder.HasIndex(x => new { x.BusinessId, x.CreatedAt });

        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}