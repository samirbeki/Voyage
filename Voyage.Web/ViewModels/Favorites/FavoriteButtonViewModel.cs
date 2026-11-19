namespace Voyage.Web.ViewModels.Favorites
{
    public class FavoriteButtonViewModel
    {
        public int DestinationId { get; set; }

        public string DestinationName { get; set; } = null!;

        public bool IsFavorite { get; set; }

        public bool Compact { get; set; }
    }
}