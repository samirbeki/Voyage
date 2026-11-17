using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Voyage.Web.Common;

namespace Voyage.Web.Infrastructure
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task SeedRolesAndAdminAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var roles = new[]
            {
                RoleConstants.Administrator,
                RoleConstants.Moderator,
                RoleConstants.TravelExpert,
                RoleConstants.User
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var email = configuration["AdminUser:Email"];
            var password = configuration["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var admin = await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, password);

                if (!result.Succeeded)
                {
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(admin, RoleConstants.Administrator))
            {
                await userManager.AddToRoleAsync(admin, RoleConstants.Administrator);
            }

            await AssignUserRoleToUsersWithoutRoleAsync(userManager);
        }

        private static async Task AssignUserRoleToUsersWithoutRoleAsync(UserManager<IdentityUser> userManager)
        {
            var users = await userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                if (roles.Count == 0)
                {
                    await userManager.AddToRoleAsync(user, RoleConstants.User);
                }
            }
        }
    }
}