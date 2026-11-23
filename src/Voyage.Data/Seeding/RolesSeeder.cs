using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Voyage.Common.Constants;

namespace Voyage.Data.Seeding
{
    public class RolesSeeder : ISeeder
    {
        private static readonly string[] Roles =
        {
            RoleConstants.Administrator,
            RoleConstants.Moderator,
            RoleConstants.TravelExpert,
            RoleConstants.User
        };

        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
