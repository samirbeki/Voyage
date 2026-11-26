using Voyage.Web.ViewModels.Enums;
using Voyage.Web.ViewModels.Expenses;

namespace Voyage.Web.ViewModels.Trips
{
    public class SharedTripViewModel
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string OwnerName { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int DaysCount { get; set; }

        public TripStatus Status { get; set; }

        public string? CoverImageUrl { get; set; }

        public IEnumerable<TripDestinationViewModel> Destinations { get; set; }
            = new List<TripDestinationViewModel>();

        public IEnumerable<ExpenseCategorySummaryViewModel> ExpensesByCategory { get; set; }
            = new List<ExpenseCategorySummaryViewModel>();

        public decimal TotalSpent { get; set; }
    }
}
