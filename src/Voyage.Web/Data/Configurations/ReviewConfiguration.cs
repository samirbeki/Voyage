using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voyage.Web.Data.Entities;

namespace Voyage.Web.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasQueryFilter(r => !r.IsDeleted);

            builder.Property(r => r.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(r => r.Destination)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Author)
                .WithMany()
                .HasForeignKey(r => r.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.AuthorId, r.DestinationId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Review_Rating", "[Rating] BETWEEN 1 AND 5"));
        }
    }
}