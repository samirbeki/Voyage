using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Voyage.Web.Common;
using Voyage.Web.Data;
using Voyage.Web.Data.Entities;
using Voyage.Web.Services.Contracts;

namespace Voyage.Web.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDbContext dbContext;

        public FavoriteService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<FavoriteResult> AddAsync(int destinationId, string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            if (destinationId <= 0 ||
                !await dbContext.Destinations.AnyAsync(d => d.Id == destinationId))
            {
                return FavoriteResult.DestinationNotFound;
            }

            if (await IsFavoriteAsync(destinationId, userId))
            {
                return FavoriteResult.AlreadyExists;
            }

            if (await GetUserFavoritesCountAsync(userId) >=
                ValidationConstants.FavoritesMaxPerUser)
            {
                return FavoriteResult.LimitReached;
            }

            var favorite = new Favorite
            {
                UserId = userId,
                DestinationId = destinationId,
                AddedOn = DateTime.UtcNow
            };

            await dbContext.Favorites.AddAsync(favorite);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException { Number: 2627 or 2601 })
            {
                dbContext.Entry(favorite).State = EntityState.Detached;
                return FavoriteResult.AlreadyExists;
            }

            return FavoriteResult.Success;
        }

        public async Task<FavoriteResult> RemoveAsync(int destinationId, string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var favorite = await dbContext.Favorites
                .FirstOrDefaultAsync(f =>
                    f.DestinationId == destinationId &&
                    f.UserId == userId);

            if (favorite == null)
            {
                return FavoriteResult.NotInFavorites;
            }

            dbContext.Favorites.Remove(favorite);
            await dbContext.SaveChangesAsync();

            return FavoriteResult.Success;
        }

        public async Task<bool> IsFavoriteAsync(int destinationId, string userId)
        {
            return await dbContext.Favorites
                .AnyAsync(f =>
                    f.DestinationId == destinationId &&
                    f.UserId == userId);
        }

        public async Task<HashSet<int>> GetFavoriteDestinationIdsAsync(
            string userId, IEnumerable<int> destinationIds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var ids = destinationIds.Distinct().ToList();

            if (ids.Count == 0)
            {
                return new HashSet<int>();
            }

            var favoriteIds = await dbContext.Favorites
                .AsNoTracking()
                .Where(f =>
                    f.UserId == userId &&
                    ids.Contains(f.DestinationId))
                .Select(f => f.DestinationId)
                .ToListAsync();

            return favoriteIds.ToHashSet();
        }

        public async Task<int> GetCountForDestinationAsync(int destinationId)
        {
            return await dbContext.Favorites
                .CountAsync(f => f.DestinationId == destinationId);
        }

        public async Task<int> GetUserFavoritesCountAsync(string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            return await dbContext.Favorites
                .CountAsync(f => f.UserId == userId);
        }
    }
}