using Voyage.Web.ViewModels.ExpertTips;
using Voyage.Web.ViewModels.Reviews;

namespace Voyage.Web.ViewModels.Destinations
{
    public class DestinationDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Country { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;

        public string CategoryName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public IEnumerable<string> DescriptionParagraphs
            => Description.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        public int CategoryId { get; set; }

        public IEnumerable<DestinationAllViewModel> SimilarDestinations { get; set; } = new List<DestinationAllViewModel>();

        public IEnumerable<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();

        public RatingSummaryViewModel RatingSummary { get; set; } = new();

        public bool CanReview { get; set; }

        public ReviewFormModel ReviewForm { get; set; } = new();

        public IEnumerable<ExpertTipViewModel> ExpertTips { get; set; } = new List<ExpertTipViewModel>();

        public bool CanAddExpertTip { get; set; }

        public ExpertTipFormModel ExpertTipForm { get; set; } = new();

        public bool IsFavorite { get; set; }

        public int FavoritesCount { get; set; }
    }
}
