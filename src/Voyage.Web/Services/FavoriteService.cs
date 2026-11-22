using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Common.Constants.Validation;
using Voyage.Data;
using Voyage.Data.Extensions;
using Voyage.Data.Models;
using Voyage.Web.Common;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Favorites;

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

            await using var transaction = await dbContext.Database
                .BeginLockedTransactionAsync($"favorites:{userId}");

            if (await IsFavoriteAsync(destinationId, userId))
            {
                return FavoriteResult.AlreadyExists;
            }

            if (await GetUserFavoritesCountAsync(userId) >=
                FavoriteValidationConstants.MaxPerUser)
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

            if (transaction != null)
            {
                await transaction.CommitAsync();
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

        public async Task<FavoritesPageViewModel> GetMineAsync(
            string userId, FavoritesQueryModel query)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            query ??= new FavoritesQueryModel();

            var search = query.Search?.Trim();

            IQueryable<Favorite> favorites = dbContext.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                favorites = favorites.Where(f =>
                    f.Destination.Name.Contains(search) ||
                    f.Destination.Country.Contains(search));
            }

            var sorting = Enum.IsDefined(query.Sorting)
                ? query.Sorting
                : FavoritesSorting.Recent;

            favorites = sorting switch
            {
                FavoritesSorting.Oldest => favorites
                    .OrderBy(f => f.AddedOn)
                    .ThenBy(f => f.Destination.Name),

                FavoritesSorting.Name => favorites
                    .OrderBy(f => f.Destination.Name),

                FavoritesSorting.Rating => favorites
                    .OrderByDescending(f =>
                        f.Destination.Reviews
                            .Where(r => !r.IsHidden)
                            .Average(r => (double?)r.Rating) ?? 0)
                    .ThenBy(f => f.Destination.Name),

                _ => favorites
                    .OrderByDescending(f => f.AddedOn)
                    .ThenBy(f => f.Destination.Name)
            };

            var resultsCount = await favorites.CountAsync();

            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    resultsCount / (double)PagingConstants.FavoritesPerPage));

            var currentPage = Math.Clamp(query.Page, 1, totalPages);

            var destinations = await favorites
                .Skip((currentPage - 1) * PagingConstants.FavoritesPerPage)
                .Take(PagingConstants.FavoritesPerPage)
                .Select(f => f.Destination)
                .Select(DestinationProjections.ToCard)
                .ToListAsync();

            foreach (var destination in destinations)
            {
                destination.IsFavorite = true;
            }

            var totalSaved = string.IsNullOrWhiteSpace(search)
                ? resultsCount
                : await GetUserFavoritesCountAsync(userId);

            return new FavoritesPageViewModel
            {
                Destinations = destinations,
                Search = search,
                Sorting = sorting,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                ResultsCount = resultsCount,
                TotalSaved = totalSaved,
                MaxFavorites = FavoriteValidationConstants.MaxPerUser
            };
        }
    }
}