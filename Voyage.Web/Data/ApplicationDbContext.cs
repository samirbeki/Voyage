using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Voyage.Web.Data.Entities;

namespace Voyage.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Destination> Destinations { get; set; } = null!;
    }
}