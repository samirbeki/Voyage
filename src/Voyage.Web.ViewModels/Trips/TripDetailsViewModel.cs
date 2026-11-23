using Voyage.Web.ViewModels.Enums;
using Voyage.Web.ViewModels.Expenses;
using Voyage.Web.ViewModels.Packing;

namespace Voyage.Web.ViewModels.Trips
{
    public class TripDetailsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Budget { get; set; }

        public string? CoverImageUrl { get; set; }

        public DateTime CreatedOn { get; set; }

        public int DaysCount { get; set; }

        public decimal BudgetPerDay { get; set; }

        public TripStatus Status { get; set; }

        public int DaysUntilStart { get; set; }

        public int CurrentDay { get; set; }

        public int DaysSinceEnd { get; set; }

        public IEnumerable<TripDestinationViewModel> Destinations { get; set; }
            = new List<TripDestinationViewModel>();

        public IEnumerable<ExpenseViewModel> Expenses { get; set; }
            = new List<ExpenseViewModel>();

        public decimal TotalSpent { get; set; }

        public decimal RemainingBudget { get; set; }

        public int SpentPercent { get; set; }

        public PackingProgressViewModel PackingProgress { get; set; } = new();
    }
}
