namespace Voyage.Web.ViewModels.Packing
{
    public class PackingListViewModel
    {
        public int TripId { get; set; }

        public string TripName { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public IEnumerable<PackingCategoryGroupViewModel> Groups { get; set; }
            = new List<PackingCategoryGroupViewModel>();

        public PackingProgressViewModel Progress { get; set; } = new();

        public int MaxItems { get; set; }

        public PackingItemFormModel NewItem { get; set; } = new();
    }
}
