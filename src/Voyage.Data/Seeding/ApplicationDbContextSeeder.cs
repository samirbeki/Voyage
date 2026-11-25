namespace Voyage.Data.Seeding
{
    public class ApplicationDbContextSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var seeders = new List<ISeeder>
            {
                new RolesSeeder(),
                new AdministratorSeeder(),
                new ModeratorSeeder(),
                new DemoUsersSeeder(),
                new TravelExpertSeeder(),
                new TripSeeder(),
                new ReviewSeeder(),
                new FavoriteSeeder(),
                new DefaultUserRoleSeeder()
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(dbContext, serviceProvider);
            }
        }
    }
}
