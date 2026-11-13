using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Trips
{
    public class TripAllViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Budget { get; set; }

        public string? CoverImageUrl { get; set; }

        public int DestinationsCount { get; set; }

        public TripStatus Status { get; set; }
    }
}