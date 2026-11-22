using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voyage.Web.Data.Entities;

namespace Voyage.Web.Data.Configurations
{
    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.HasKey(f => new { f.UserId, f.DestinationId });

            builder.HasQueryFilter(f => !f.Destination.IsDeleted);

            builder.Property(f => f.AddedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Destination)
                .WithMany(d => d.Favorites)
                .HasForeignKey(f => f.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(f => f.DestinationId);
        }
    }
}