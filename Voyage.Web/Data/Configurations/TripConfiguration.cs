using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voyage.Web.Data.Entities;

namespace Voyage.Web.Data.Configurations
{
    public class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> builder)
        {
            builder.HasIndex(t => t.OwnerId);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Trips_EndDate",
                    "[EndDate] >= [StartDate]");

                t.HasCheckConstraint(
                    "CK_Trips_Budget",
                    "[Budget] > 0");
            });
        }
    }
}