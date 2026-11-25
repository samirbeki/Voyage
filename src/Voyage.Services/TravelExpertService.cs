using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Common.Utilities;
using Voyage.Data;
using Voyage.Services.Contracts;
using Voyage.Web.ViewModels.TravelExperts;

namespace Voyage.Services
{
    public class TravelExpertService : ITravelExpertService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public TravelExpertService(
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<TravelExpertsPageViewModel> GetUsersAsync(string? search)
        {
            var adminRoleId = await GetRoleIdAsync(RoleConstants.Administrator);
            var expertRoleId = await GetRoleIdAsync(RoleConstants.TravelExpert);
            var term = TextNormalizer.SingleLine(search);

            var query = dbContext.Users
                .AsNoTracking()
                .Where(u => !dbContext.UserRoles
                    .Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId));

            if (term.Length > 0)
            {
                var lowerTerm = term.ToLower();

                query = query.Where(u => u.Email!.ToLower().Contains(lowerTerm));
            }

            var users = await query
                .Select(u => new TravelExpertUserViewModel
                {
                    Id = u.Id,
                    Email = u.Email!,
                    ReviewsCount = dbContext.Reviews.Count(r => r.AuthorId == u.Id),
                    IsExpert = dbContext.UserRoles
                        .Any(ur => ur.UserId == u.Id && ur.RoleId == expertRoleId)
                })
                .ToListAsync();

            return new TravelExpertsPageViewModel
            {
                Search = term.Length > 0 ? term : null,
                ExpertsCount = await dbContext.UserRoles
                    .CountAsync(ur => ur.RoleId == expertRoleId),
                Users = users
                    .OrderByDescending(u => u.IsExpert)
                    .ThenBy(u => u.Email)
                    .ToList()
            };
        }

        public async Task<string?> GrantAsync(string userId)
        {
            var user = await FindCandidateAsync(userId);

            if (user == null || await userManager.IsInRoleAsync(user, RoleConstants.TravelExpert))
            {
                return null;
            }

            var result = await userManager.AddToRoleAsync(user, RoleConstants.TravelExpert);

            return result.Succeeded ? user.Email : null;
        }

        public async Task<string?> RemoveAsync(string userId)
        {
            var user = await FindCandidateAsync(userId);

            if (user == null || !await userManager.IsInRoleAsync(user, RoleConstants.TravelExpert))
            {
                return null;
            }

            var result = await userManager.RemoveFromRoleAsync(user, RoleConstants.TravelExpert);

            return result.Succeeded ? user.Email : null;
        }

        private async Task<IdentityUser?> FindCandidateAsync(string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var user = await userManager.FindByIdAsync(userId);

            if (user == null || await userManager.IsInRoleAsync(user, RoleConstants.Administrator))
            {
                return null;
            }

            return user;
        }

        private async Task<string?> GetRoleIdAsync(string roleName)
        {
            return await dbContext.Roles
                .Where(r => r.Name == roleName)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();
        }
    }
}
