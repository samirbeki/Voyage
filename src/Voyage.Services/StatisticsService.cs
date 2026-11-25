using Microsoft.EntityFrameworkCore;
using Voyage.Data;
using Voyage.Services.Contracts;
using Voyage.Services.Helpers;
using Voyage.Web.ViewModels.Statistics;

namespace Voyage.Services
{
    public class StatisticsService : IStatisticsService
    {
        private const int TopCountriesCount = 5;

        private readonly ApplicationDbContext dbContext;

        public StatisticsService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<UserStatisticsViewModel> GetForUserAsync(string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var trips = await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.OwnerId == userId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.StartDate,
                    t.EndDate,
                    t.Budget,
                    Destinations = t.TripDestinations
                        .Where(td => !td.Destination.IsDeleted)
                        .Select(td => new { td.DestinationId, td.Destination.Country })
                        .ToList(),
                    Expenses = t.Expenses
                        .Select(e => new { e.Category, e.Amount })
                        .ToList()
                })
                .ToListAsync();

            var today = DateTime.Today;
            var model = new UserStatisticsViewModel
            {
                TripsCount = trips.Count,
                UpcomingCount = trips.Count(t => t.StartDate > today),
                OngoingCount = trips.Count(t => t.StartDate <= today && t.EndDate >= today),
                CompletedCount = trips.Count(t => t.EndDate < today)
            };

            var startedTrips = trips.Where(t => t.StartDate <= today).ToList();

            model.DaysTraveled = startedTrips.Sum(t =>
                ((t.EndDate < today ? t.EndDate : today) - t.StartDate).Days + 1);

            model.DestinationsVisited = startedTrips
                .SelectMany(t => t.Destinations)
                .Select(d => d.DestinationId)
                .Distinct()
                .Count();

            model.CountriesVisited = startedTrips
                .SelectMany(t => t.Destinations)
                .Select(d => d.Country)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            model.TotalBudget = trips.Sum(t => t.Budget);
            model.TotalSpent = trips.Sum(t => t.Expenses.Sum(e => e.Amount));

            model.BudgetUsedPercent = model.TotalBudget > 0
                ? (int)Math.Min(model.TotalSpent / model.TotalBudget * 100, 100m)
                : 0;

            var tripsWithExpenses = trips.Where(t => t.Expenses.Any()).ToList();
            var daysWithExpenses = tripsWithExpenses.Sum(t => (t.EndDate - t.StartDate).Days + 1);

            model.AverageSpentPerDay = daysWithExpenses > 0
                ? Math.Round(model.TotalSpent / daysWithExpenses, 2)
                : 0;

            model.TripsOverBudget = trips.Count(t => t.Expenses.Sum(e => e.Amount) > t.Budget);

            model.ExpensesByCategory = ExpenseCategorySummaries.Create(
                trips.SelectMany(t => t.Expenses).Select(e => (e.Category, e.Amount)));

            var countryTrips = trips
                .SelectMany(t => t.Destinations
                    .Select(d => d.Country)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(country => new { TripId = t.Id, Country = country }))
                .GroupBy(x => x.Country, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Country = g.Key, TripsCount = g.Count() })
                .OrderByDescending(x => x.TripsCount)
                .ThenBy(x => x.Country)
                .Take(TopCountriesCount)
                .ToList();

            model.TopCountries = countryTrips
                .Select(x => new CountryStatisticViewModel
                {
                    Country = x.Country,
                    TripsCount = x.TripsCount,
                    Percent = (int)Math.Round(x.TripsCount * 100.0 / trips.Count)
                })
                .ToList();

            model.NextTrip = trips
                .Where(t => t.StartDate > today)
                .OrderBy(t => t.StartDate)
                .ThenBy(t => t.Id)
                .Select(t => new TripHighlightViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    DaysUntilStart = (t.StartDate - today).Days,
                    Spent = t.Expenses.Sum(e => e.Amount)
                })
                .FirstOrDefault();

            model.MostExpensiveTrip = trips
                .Where(t => t.Expenses.Any())
                .OrderByDescending(t => t.Expenses.Sum(e => e.Amount))
                .ThenBy(t => t.StartDate)
                .Select(t => new TripHighlightViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Spent = t.Expenses.Sum(e => e.Amount)
                })
                .FirstOrDefault();

            model.ReviewsCount = await dbContext.Reviews
                .CountAsync(r => r.AuthorId == userId && !r.Destination.IsDeleted);

            model.AverageRatingGiven = await dbContext.Reviews
                .Where(r => r.AuthorId == userId && !r.Destination.IsDeleted)
                .Select(r => (double?)r.Rating)
                .AverageAsync();

            if (model.AverageRatingGiven.HasValue)
            {
                model.AverageRatingGiven = Math.Round(model.AverageRatingGiven.Value, 1);
            }

            model.FavoritesCount = await dbContext.Favorites
                .CountAsync(f => f.UserId == userId);

            return model;
        }
    }
}
