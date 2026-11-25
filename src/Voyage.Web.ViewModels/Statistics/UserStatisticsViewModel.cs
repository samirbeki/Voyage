using Voyage.Web.ViewModels.Expenses;

namespace Voyage.Web.ViewModels.Statistics
{
    public class UserStatisticsViewModel
    {
        public int TripsCount { get; set; }

        public int UpcomingCount { get; set; }

        public int OngoingCount { get; set; }

        public int CompletedCount { get; set; }

        public int DaysTraveled { get; set; }

        public int DestinationsVisited { get; set; }

        public int CountriesVisited { get; set; }

        public decimal TotalBudget { get; set; }

        public decimal TotalSpent { get; set; }

        public int BudgetUsedPercent { get; set; }

        public decimal AverageSpentPerDay { get; set; }

        public int TripsOverBudget { get; set; }

        public IEnumerable<ExpenseCategorySummaryViewModel> ExpensesByCategory { get; set; }
            = new List<ExpenseCategorySummaryViewModel>();

        public IEnumerable<CountryStatisticViewModel> TopCountries { get; set; }
            = new List<CountryStatisticViewModel>();

        public TripHighlightViewModel? NextTrip { get; set; }

        public TripHighlightViewModel? MostExpensiveTrip { get; set; }

        public int ReviewsCount { get; set; }

        public double? AverageRatingGiven { get; set; }

        public int FavoritesCount { get; set; }
    }
}
