using Voyage.Web.ViewModels.Reviews;

namespace Voyage.Web.ViewModels.Destinations
{
    public class DestinationDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Country { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int CategoryId { get; set; }

        public IEnumerable<DestinationAllViewModel> SimilarDestinations { get; set; } = new List<DestinationAllViewModel>();

        public IEnumerable<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();

        public RatingSummaryViewModel RatingSummary { get; set; } = new();

        public bool CanReview { get; set; }

        public ReviewFormModel ReviewForm { get; set; } = new();

        public bool IsFavorite { get; set; }
    }
}