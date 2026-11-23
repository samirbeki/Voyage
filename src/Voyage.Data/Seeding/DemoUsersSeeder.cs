using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voyage.Common.Constants;

namespace Voyage.Data.Seeding
{
    public class DemoUsersSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            foreach (var section in configuration.GetSection("DemoUsers").GetChildren())
            {
                await UserSeedingHelper.EnsureUserInRoleAsync(
                    userManager,
                    section["Email"],
                    section["Password"],
                    RoleConstants.User);
            }
        }
    }
}
