using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Voyage.Web.Data.Entities;

namespace Voyage.Web.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Destination> Destinations { get; set; } = null!;

        public DbSet<Trip> Trips { get; set; } = null!;

        public DbSet<TripDestination> TripDestinations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}