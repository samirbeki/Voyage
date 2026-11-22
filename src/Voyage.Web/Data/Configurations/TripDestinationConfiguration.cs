using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voyage.Web.Data.Entities;

namespace Voyage.Web.Data.Configurations
{
    public class TripDestinationConfiguration : IEntityTypeConfiguration<TripDestination>
    {
        public void Configure(EntityTypeBuilder<TripDestination> builder)
        {
            builder.HasIndex(td => td.DestinationId);

            builder.HasQueryFilter(td => !td.Destination.IsDeleted);
        }
    }
}