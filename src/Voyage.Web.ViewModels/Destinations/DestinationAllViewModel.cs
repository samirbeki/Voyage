namespace Voyage.Web.ViewModels.Destinations
{
    public class DestinationAllViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Country { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public double? AverageRating { get; set; }

        public int ReviewsCount { get; set; }

        public bool IsFavorite { get; set; }
    }
}
