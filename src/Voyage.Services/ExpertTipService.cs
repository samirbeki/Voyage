using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Services.Contracts;
using Voyage.Services.Results;
using Voyage.Web.ViewModels.ExpertTips;

namespace Voyage.Services
{
    public class ExpertTipService : IExpertTipService
    {
        private readonly ApplicationDbContext dbContext;

        public ExpertTipService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<ExpertTipViewModel>> GetForDestinationAsync(
            int destinationId, string? userId)
        {
            var expertRoleId = await GetExpertRoleIdAsync();

            if (expertRoleId == null)
            {
                return new List<ExpertTipViewModel>();
            }

            var tips = await dbContext.ExpertTips
                .AsNoTracking()
                .Where(t =>
                    t.DestinationId == destinationId &&
                    dbContext.UserRoles.Any(ur => ur.UserId == t.AuthorId && ur.RoleId == expertRoleId))
                .OrderByDescending(t => t.CreatedOn)
                .Select(t => new ExpertTipViewModel
                {
                    Id = t.Id,
                    Content = t.Content,
                    AuthorName = t.Author.UserName!,
                    CreatedOn = t.CreatedOn,
                    IsEdited = t.ModifiedOn != null,
                    IsMine = userId != null && t.AuthorId == userId
                })
                .ToListAsync();

            foreach (var tip in tips)
            {
                int atIndex = tip.AuthorName.IndexOf('@');

                if (atIndex > 0)
                {
                    tip.AuthorName = tip.AuthorName[..atIndex];
                }
            }

            return tips;
        }

        public async Task<bool> CanAddAsync(int destinationId, string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            return await dbContext.Destinations.AnyAsync(d => d.Id == destinationId) &&
                   await IsExpertAsync(userId) &&
                   !await dbContext.ExpertTips.AnyAsync(t =>
                       t.DestinationId == destinationId && t.AuthorId == userId);
        }

        public async Task<ExpertTipResult> CreateAsync(
            int destinationId, ExpertTipFormModel model, string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            if (!await dbContext.Destinations.AnyAsync(d => d.Id == destinationId))
            {
                return ExpertTipResult.DestinationNotFound;
            }

            if (!await IsExpertAsync(userId))
            {
                return ExpertTipResult.NotExpert;
            }

            if (await dbContext.ExpertTips.AnyAsync(t =>
                    t.DestinationId == destinationId && t.AuthorId == userId))
            {
                return ExpertTipResult.AlreadyExists;
            }

            var tip = new ExpertTip
            {
                DestinationId = destinationId,
                AuthorId = userId,
                Content = model.Content.Trim(),
                CreatedOn = DateTime.UtcNow
            };

            await dbContext.ExpertTips.AddAsync(tip);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException { Number: 2627 or 2601 })
            {
                dbContext.Entry(tip).State = EntityState.Detached;

                return ExpertTipResult.AlreadyExists;
            }

            return ExpertTipResult.Success;
        }

        public async Task<ExpertTipEditViewModel?> GetForEditAsync(int id, string userId)
        {
            return await dbContext.ExpertTips
                .AsNoTracking()
                .Where(t => t.Id == id && t.AuthorId == userId)
                .Select(t => new ExpertTipEditViewModel
                {
                    Id = t.Id,
                    DestinationId = t.DestinationId,
                    DestinationName = t.Destination.Name,
                    Content = t.Content
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int?> EditAsync(int id, ExpertTipFormModel model, string userId)
        {
            var tip = await dbContext.ExpertTips
                .FirstOrDefaultAsync(t => t.Id == id && t.AuthorId == userId);

            if (tip == null)
            {
                return null;
            }

            tip.Content = model.Content.Trim();
            tip.ModifiedOn = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return tip.DestinationId;
        }

        public async Task<int?> DeleteAsync(int id, string userId)
        {
            var tip = await dbContext.ExpertTips
                .FirstOrDefaultAsync(t => t.Id == id && t.AuthorId == userId);

            if (tip == null)
            {
                return null;
            }

            dbContext.ExpertTips.Remove(tip);
            await dbContext.SaveChangesAsync();

            return tip.DestinationId;
        }

        public async Task<int?> DeleteByModeratorAsync(int id)
        {
            var tip = await dbContext.ExpertTips.FirstOrDefaultAsync(t => t.Id == id);

            if (tip == null)
            {
                return null;
            }

            dbContext.ExpertTips.Remove(tip);
            await dbContext.SaveChangesAsync();

            return tip.DestinationId;
        }

        private async Task<bool> IsExpertAsync(string userId)
        {
            var expertRoleId = await GetExpertRoleIdAsync();

            return expertRoleId != null &&
                   await dbContext.UserRoles.AnyAsync(ur =>
                       ur.UserId == userId && ur.RoleId == expertRoleId);
        }

        private async Task<string?> GetExpertRoleIdAsync()
        {
            return await dbContext.Roles
                .Where(r => r.Name == RoleConstants.TravelExpert)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();
        }
    }
}
