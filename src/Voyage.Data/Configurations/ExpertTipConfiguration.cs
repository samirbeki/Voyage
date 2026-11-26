using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voyage.Data.Models;

namespace Voyage.Data.Configurations
{
    public class ExpertTipConfiguration : IEntityTypeConfiguration<ExpertTip>
    {
        public void Configure(EntityTypeBuilder<ExpertTip> builder)
        {
            builder.HasQueryFilter(t => !t.Destination.IsDeleted);

            builder.Property(t => t.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(t => t.Destination)
                .WithMany(d => d.ExpertTips)
                .HasForeignKey(t => t.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Author)
                .WithMany()
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => new { t.AuthorId, t.DestinationId })
                .IsUnique();
        }
    }
}
