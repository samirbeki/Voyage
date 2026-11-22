using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voyage.Common.Constants.Validation;
using Voyage.Data.Models;

namespace Voyage.Data.Configurations
{
    public class PackingItemConfiguration : IEntityTypeConfiguration<PackingItem>
    {
        public void Configure(EntityTypeBuilder<PackingItem> builder)
        {
            builder.Property(p => p.Quantity)
                .HasDefaultValue(1);

            builder.Property(p => p.IsPacked)
                .HasDefaultValue(false);

            builder.Property(p => p.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(p => p.Trip)
                .WithMany(t => t.PackingItems)
                .HasForeignKey(p => p.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => new { p.TripId, p.Name })
                .IsUnique();

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_PackingItem_Quantity",
                    $"[Quantity] BETWEEN {PackingItemValidationConstants.QuantityMin} AND {PackingItemValidationConstants.QuantityMax}");

                t.HasCheckConstraint(
                    "CK_PackingItem_Category",
                    "[Category] BETWEEN 0 AND 5");
            });
        }
    }
}
