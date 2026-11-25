namespace Voyage.Web.ViewModels.Statistics
{
    public class TripHighlightViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int DaysUntilStart { get; set; }

        public decimal Spent { get; set; }
    }
}
