using Voyage.Services.Results;
using Voyage.Web.ViewModels.Favorites;

namespace Voyage.Services.Contracts
{
    public interface IFavoriteService
    {
        Task<FavoriteResult> AddAsync(int destinationId, string userId);

        Task<FavoriteResult> RemoveAsync(int destinationId, string userId);

        Task<bool> IsFavoriteAsync(int destinationId, string userId);

        Task<HashSet<int>> GetFavoriteDestinationIdsAsync(
            string userId, IEnumerable<int> destinationIds);

        Task<int> GetCountForDestinationAsync(int destinationId);

        Task<int> GetUserFavoritesCountAsync(string userId);

        Task<FavoritesPageViewModel> GetMineAsync(
            string userId, FavoritesQueryModel query);
    }
}
