using Voyage.Data;
using Voyage.Data.Seeding;

namespace Voyage.Web.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await new ApplicationDbContextSeeder().SeedAsync(dbContext, scope.ServiceProvider);
        }
    }
}
