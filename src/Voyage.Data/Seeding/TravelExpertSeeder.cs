using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Voyage.Common.Constants;

namespace Voyage.Data.Seeding
{
    public class TravelExpertSeeder : ISeeder
    {
        private static readonly string[] ExpertEmails =
        {
            "maria@voyage.test",
            "georgi@voyage.test"
        };

        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var existingExperts = await userManager.GetUsersInRoleAsync(RoleConstants.TravelExpert);

            if (existingExperts.Any())
            {
                return;
            }

            foreach (var email in ExpertEmails)
            {
                var user = await userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    await userManager.AddToRoleAsync(user, RoleConstants.TravelExpert);
                }
            }
        }
    }
}
