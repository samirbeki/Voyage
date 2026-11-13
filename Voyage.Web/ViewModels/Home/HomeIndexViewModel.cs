using Voyage.Web.ViewModels.Destinations;

namespace Voyage.Web.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public IEnumerable<DestinationAllViewModel> PopularDestinations { get; set; }
            = new List<DestinationAllViewModel>();

        public int DestinationsCount { get; set; }

        public int CategoriesCount { get; set; }

        public int TripsCount { get; set; }
    }
}