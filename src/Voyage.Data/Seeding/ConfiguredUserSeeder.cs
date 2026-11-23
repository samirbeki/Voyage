using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Voyage.Data.Seeding
{
    public abstract class ConfiguredUserSeeder : ISeeder
    {
        protected abstract string ConfigurationSection { get; }

        protected abstract string RoleName { get; }

        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            await UserSeedingHelper.EnsureUserInRoleAsync(
                userManager,
                configuration[$"{ConfigurationSection}:Email"],
                configuration[$"{ConfigurationSection}:Password"],
                RoleName);
        }
    }
}
