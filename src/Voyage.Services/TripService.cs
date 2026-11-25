using Microsoft.EntityFrameworkCore;
using Voyage.Common.Utilities;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Services.Contracts;
using Voyage.Web.ViewModels.Enums;
using Voyage.Web.ViewModels.Expenses;
using Voyage.Web.ViewModels.Trips;

namespace Voyage.Services
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext dbContext;

        public TripService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<TripAllViewModel>> GetMineAsync(string userId)
        {
            var today = DateTime.Today;

            return await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.OwnerId == userId)
                .OrderBy(t => t.StartDate)
                .Select(t => new TripAllViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Budget = t.Budget,
                    CoverImageUrl = t.CoverImageUrl == null || t.CoverImageUrl == ""
                        ? t.TripDestinations
                            .OrderBy(td => td.Destination.Name)
                            .Select(td => td.Destination.ImageUrl)
                            .FirstOrDefault()
                        : t.CoverImageUrl,
                    DestinationsCount = t.TripDestinations.Count,
                    Status = t.EndDate < today
                        ? TripStatus.Completed
                        : t.StartDate > today
                            ? TripStatus.Upcoming
                            : TripStatus.Ongoing
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DestinationOptionViewModel>> GetDestinationOptionsAsync()
        {
            return await dbContext.Destinations
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new DestinationOptionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Country = d.Country
                })
                .ToListAsync();
        }

        public async Task<bool> DestinationsExistAsync(IEnumerable<int> destinationIds)
        {
            var ids = destinationIds.Distinct().ToList();

            var count = await dbContext.Destinations
                .CountAsync(d => ids.Contains(d.Id) && !d.IsDeleted);

            return count == ids.Count;
        }

        public async Task<int> CreateAsync(TripFormViewModel model, string userId)
        {
            var trip = new Trip
            {
                Title = TextNormalizer.SingleLine(model.Title),
                Description = model.Description?.Trim(),
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Budget = model.Budget!.Value,
                CoverImageUrl = model.CoverImageUrl?.Trim(),
                OwnerId = userId,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var id in model.SelectedDestinationIds.Distinct())
            {
                trip.TripDestinations.Add(new TripDestination
                {
                    DestinationId = id
                });
            }

            await dbContext.Trips.AddAsync(trip);
            await dbContext.SaveChangesAsync();

            return trip.Id;
        }

        public async Task<TripDetailsViewModel?> GetDetailsAsync(int id, string userId)
        {
            var trip = await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.Id == id && t.OwnerId == userId)
                .Select(t => new TripDetailsViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Budget = t.Budget,
                    CoverImageUrl = t.CoverImageUrl == null || t.CoverImageUrl == ""
                        ? t.TripDestinations
                            .OrderBy(td => td.Destination.Name)
                            .Select(td => td.Destination.ImageUrl)
                            .FirstOrDefault()
                        : t.CoverImageUrl,
                    CreatedOn = t.CreatedOn,
                    Destinations = t.TripDestinations
                        .Where(td => !td.Destination.IsDeleted)
                        .OrderBy(td => td.Destination.Name)
                        .Select(td => new TripDestinationViewModel
                        {
                            Id = td.Destination.Id,
                            Name = td.Destination.Name,
                            Country = td.Destination.Country,
                            ImageUrl = td.Destination.ImageUrl
                        })
                        .ToList(),
                    Expenses = t.Expenses
                        .OrderByDescending(e => e.Date)
                        .ThenByDescending(e => e.Id)
                        .Select(e => new ExpenseViewModel
                        {
                            Id = e.Id,
                            TripId = e.TripId,
                            Title = e.Title,
                            Amount = e.Amount,
                            Category = e.Category,
                            Date = e.Date
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                return null;
            }

            trip.DaysCount = (trip.EndDate - trip.StartDate).Days + 1;

            trip.BudgetPerDay = Math.Round(
                trip.Budget / trip.DaysCount,
                2);

            trip.TotalSpent = trip.Expenses.Sum(e => e.Amount);

            trip.ExpensesByCategory = SummarizeByCategory(trip.Expenses, trip.TotalSpent);

            trip.RemainingBudget = trip.Budget - trip.TotalSpent;

            trip.SpentPercent = trip.Budget > 0
                ? (int)Math.Min(trip.TotalSpent / trip.Budget * 100, 100m)
                : 0;

            var today = DateTime.Today;

            if (trip.EndDate < today)
            {
                trip.Status = TripStatus.Completed;
                trip.DaysSinceEnd = (today - trip.EndDate).Days;
            }
            else if (trip.StartDate > today)
            {
                trip.Status = TripStatus.Upcoming;
                trip.DaysUntilStart = (trip.StartDate - today).Days;
            }
            else
            {
                trip.Status = TripStatus.Ongoing;
                trip.CurrentDay = (today - trip.StartDate).Days + 1;
            }

            return trip;
        }

        public async Task<TripFormViewModel?> GetForEditAsync(int id, string userId)
        {
            return await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.Id == id && t.OwnerId == userId)
                .Select(t => new TripFormViewModel
                {
                    Title = t.Title,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Budget = t.Budget,
                    CoverImageUrl = t.CoverImageUrl,
                    SelectedDestinationIds = t.TripDestinations
                        .Select(td => td.DestinationId)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EditAsync(int id, TripFormViewModel model, string userId)
        {
            var trip = await dbContext.Trips
                .Include(t => t.TripDestinations)
                .FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == userId);

            if (trip == null)
            {
                return false;
            }

            trip.Title = TextNormalizer.SingleLine(model.Title);
            trip.Description = model.Description?.Trim();
            trip.StartDate = model.StartDate;
            trip.EndDate = model.EndDate;
            trip.Budget = model.Budget!.Value;
            trip.CoverImageUrl = model.CoverImageUrl?.Trim();

            var selectedIds = model.SelectedDestinationIds.Distinct().ToList();

            var destinationsToRemove = trip.TripDestinations
                .Where(td => !selectedIds.Contains(td.DestinationId))
                .ToList();

            dbContext.TripDestinations.RemoveRange(destinationsToRemove);

            foreach (var destinationId in selectedIds)
            {
                if (!trip.TripDestinations.Any(
                    td => td.DestinationId == destinationId))
                {
                    trip.TripDestinations.Add(new TripDestination
                    {
                        DestinationId = destinationId
                    });
                }
            }

            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> HasExpensesOutsideAsync(
            int tripId,
            DateTime startDate,
            DateTime endDate)
        {
            var start = startDate.Date;
            var end = endDate.Date;

            return await dbContext.Expenses
                .AnyAsync(e =>
                    e.TripId == tripId &&
                    (e.Date < start || e.Date > end));
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var trip = await dbContext.Trips
                .FirstOrDefaultAsync(t =>
                    t.Id == id && t.OwnerId == userId);

            if (trip == null)
            {
                return false;
            }

            var packingItems = await dbContext.PackingItems
                .Where(p => p.TripId == id)
                .ToListAsync();

            dbContext.PackingItems.RemoveRange(packingItems);
            dbContext.Trips.Remove(trip);

            await dbContext.SaveChangesAsync();

            return true;
        }

        private static List<ExpenseCategorySummaryViewModel> SummarizeByCategory(
            IEnumerable<ExpenseViewModel> expenses,
            decimal totalSpent)
        {
            if (totalSpent <= 0)
            {
                return new List<ExpenseCategorySummaryViewModel>();
            }

            return expenses
                .GroupBy(e => e.Category)
                .Select(g => new ExpenseCategorySummaryViewModel
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count(),
                    Share = Math.Round(g.Sum(e => e.Amount) / totalSpent * 100, 1)
                })
                .OrderByDescending(c => c.Amount)
                .ThenBy(c => c.Category)
                .ToList();
        }
    }
}
