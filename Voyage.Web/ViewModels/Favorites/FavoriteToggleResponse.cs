namespace Voyage.Web.ViewModels.Favorites
{
    public class FavoriteToggleResponse
    {
        public bool Success { get; set; }

        public bool IsFavorite { get; set; }

        public int Count { get; set; }

        public string CountText { get; set; } = null!;

        public string Message { get; set; } = null!;
    }
}