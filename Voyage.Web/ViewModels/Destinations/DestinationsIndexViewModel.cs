namespace Voyage.Web.ViewModels.Destinations
{
    public class DestinationsIndexViewModel
    {
        public string? SearchTerm { get; set; }

        public int? CategoryId { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }

        public IEnumerable<DestinationAllViewModel> Destinations { get; set; }
            = new List<DestinationAllViewModel>();

        public IEnumerable<CategoryOptionViewModel> Categories { get; set; }
            = new List<CategoryOptionViewModel>();
    }
}