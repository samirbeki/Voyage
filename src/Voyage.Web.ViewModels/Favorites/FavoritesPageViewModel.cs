using Voyage.Web.ViewModels.Destinations;
using Voyage.Web.ViewModels.Enums;

namespace Voyage.Web.ViewModels.Favorites
{
    public class FavoritesPageViewModel
    {
        public IEnumerable<DestinationAllViewModel> Destinations { get; set; } =
            new List<DestinationAllViewModel>();

        public string? Search { get; set; }

        public FavoritesSorting Sorting { get; set; } = FavoritesSorting.Recent;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;

        public int ResultsCount { get; set; }

        public int TotalSaved { get; set; }

        public int MaxFavorites { get; set; }
    }
}
