using Microsoft.AspNetCore.Identity;

namespace Voyage.Data.Seeding
{
    internal static class UserSeedingHelper
    {
        public static async Task<IdentityUser?> EnsureUserInRoleAsync(
            UserManager<IdentityUser> userManager,
            string? email,
            string? password,
            string roleName)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    return null;
                }
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
            }

            return user;
        }
    }
}
