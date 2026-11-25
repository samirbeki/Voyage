using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Data.Models.Enums;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Enums;
using Voyage.Web.ViewModels.Trips;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class TripServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly TripService service;

        public TripServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new TripService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetMineAsync_ReturnsOnlyOwnTripsOrderedByStartDate()
        {
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            await AddTripAsync(dbContext, maria.Id, "Santorini", 60);
            await AddTripAsync(dbContext, maria.Id, "Prague", -30);
            await AddTripAsync(dbContext, georgi.Id, "Kyoto", 20);

            var result = await service.GetMineAsync(maria.Id);

            Assert.Equal(new[] { "Prague", "Santorini" }, result.Select(t => t.Title));
        }

        [Fact]
        public async Task GetMineAsync_CalculatesStatusFromTheDates()
        {
            var user = await AddUserAsync(dbContext, "ivan@voyage.test");
            await AddTripAsync(dbContext, user.Id, "Past", -20, 5);
            await AddTripAsync(dbContext, user.Id, "Current", -1, 3);
            await AddTripAsync(dbContext, user.Id, "Future", 15, 4);

            var result = (await service.GetMineAsync(user.Id)).ToDictionary(t => t.Title, t => t.Status);

            Assert.Equal(TripStatus.Completed, result["Past"]);
            Assert.Equal(TripStatus.Ongoing, result["Current"]);
            Assert.Equal(TripStatus.Upcoming, result["Future"]);
        }

        [Fact]
        public async Task GetMineAsync_WithoutCoverImage_UsesImageOfFirstDestinationByName()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var florence = await AddDestinationAsync(dbContext, category.Id, "Florence", "Italy");
            rome.ImageUrl = "/images/destinations/rome.jpg";
            florence.ImageUrl = "/images/destinations/florence.jpg";
            await dbContext.SaveChangesAsync();
            await AddTripAsync(dbContext, user.Id, "Italy", destinationIds: new[] { rome.Id, florence.Id });

            var trip = Assert.Single(await service.GetMineAsync(user.Id));

            Assert.Equal("/images/destinations/florence.jpg", trip.CoverImageUrl);
            Assert.Equal(2, trip.DestinationsCount);
        }

        [Fact]
        public async Task GetMineAsync_WithOwnCoverImage_UsesIt()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            rome.ImageUrl = "/images/destinations/rome.jpg";
            var trip = await AddTripAsync(dbContext, user.Id, "Italy", destinationIds: new[] { rome.Id });
            trip.CoverImageUrl = "https://example.com/my-photo.jpg";
            await dbContext.SaveChangesAsync();

            var result = Assert.Single(await service.GetMineAsync(user.Id));

            Assert.Equal("https://example.com/my-photo.jpg", result.CoverImageUrl);
        }

        [Fact]
        public async Task GetMineAsync_UserWithoutTrips_ReturnsEmptyCollection()
        {
            var user = await AddUserAsync(dbContext, "new@voyage.test");

            Assert.Empty(await service.GetMineAsync(user.Id));
        }

        [Fact]
        public async Task GetDestinationOptionsAsync_ReturnsActiveDestinationsOrderedByName()
        {
            var category = await AddCategoryAsync(dbContext, "Mountains");
            await AddDestinationAsync(dbContext, category.Id, "Zermatt", "Switzerland");
            await AddDestinationAsync(dbContext, category.Id, "Bansko");
            await AddDestinationAsync(dbContext, category.Id, "Aspen", "USA", isDeleted: true);

            var result = (await service.GetDestinationOptionsAsync()).ToList();

            Assert.Equal(new[] { "Bansko", "Zermatt" }, result.Select(d => d.Name));
            Assert.Equal("Switzerland", result[1].Country);
        }

        [Fact]
        public async Task DestinationsExistAsync_AllActive_ReturnsTrue()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");

            Assert.True(await service.DestinationsExistAsync(new[] { rome.Id, paris.Id }));
        }

        [Fact]
        public async Task DestinationsExistAsync_DuplicateIds_AreCountedOnce()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");

            Assert.True(await service.DestinationsExistAsync(new[] { rome.Id, rome.Id }));
        }

        [Fact]
        public async Task DestinationsExistAsync_MissingId_ReturnsFalse()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");

            Assert.False(await service.DestinationsExistAsync(new[] { rome.Id, 999 }));
        }

        [Fact]
        public async Task DestinationsExistAsync_DeletedDestination_ReturnsFalse()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var oldTown = await AddDestinationAsync(dbContext, category.Id, "Old Town", isDeleted: true);

            Assert.False(await service.DestinationsExistAsync(new[] { oldTown.Id }));
        }

        [Fact]
        public async Task CreateAsync_SavesNormalizedTripForTheOwnerWithDistinctDestinations()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Culture");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");
            var lyon = await AddDestinationAsync(dbContext, category.Id, "Lyon", "France");
            var start = DateTime.Today.AddDays(30);

            var id = await service.CreateAsync(new TripFormViewModel
            {
                Title = "  Paris   for  Two ",
                Description = "  Museums and cafes  ",
                StartDate = start,
                EndDate = start.AddDays(4),
                Budget = 1600m,
                CoverImageUrl = "  https://example.com/paris.jpg ",
                SelectedDestinationIds = new List<int> { paris.Id, lyon.Id, paris.Id }
            }, user.Id);

            var trip = await dbContext.Trips.Include(t => t.TripDestinations).SingleAsync();
            Assert.Equal(trip.Id, id);
            Assert.Equal("Paris for Two", trip.Title);
            Assert.Equal("Museums and cafes", trip.Description);
            Assert.Equal("https://example.com/paris.jpg", trip.CoverImageUrl);
            Assert.Equal(1600m, trip.Budget);
            Assert.Equal(user.Id, trip.OwnerId);
            Assert.Equal(2, trip.TripDestinations.Count);
            Assert.True(trip.CreatedOn > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task GetDetailsAsync_TripOfAnotherUser_ReturnsNull()
        {
            var owner = await AddUserAsync(dbContext, "maria@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);

            Assert.Null(await service.GetDetailsAsync(trip.Id, stranger.Id));
        }

        [Fact]
        public async Task GetDetailsAsync_MissingTrip_ReturnsNull()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");

            Assert.Null(await service.GetDetailsAsync(999, user.Id));
        }

        [Fact]
        public async Task GetDetailsAsync_CalculatesDaysAndBudgetSummary()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Weekend", 10, 3, 400m);
            AddExpense(trip, "Hotel", 150m, 0);
            AddExpense(trip, "Food", 70.50m, 1);
            await dbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(4, result.DaysCount);
            Assert.Equal(100m, result.BudgetPerDay);
            Assert.Equal(220.50m, result.TotalSpent);
            Assert.Equal(179.50m, result.RemainingBudget);
            Assert.Equal(55, result.SpentPercent);
        }

        [Fact]
        public async Task GetDetailsAsync_OverBudget_CapsPercentAtHundredAndShowsNegativeRemaining()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Paris", -10, 4, 1000m);
            AddExpense(trip, "Hotel", 900m, 0);
            AddExpense(trip, "Dinner cruise", 250m, 2);
            await dbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(100, result.SpentPercent);
            Assert.Equal(-150m, result.RemainingBudget);
        }

        [Fact]
        public async Task GetDetailsAsync_ZeroBudget_ReturnsZeroPercent()
        {
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Free hike", 5, 0, 0m);

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(0, result.SpentPercent);
            Assert.Equal(1, result.DaysCount);
        }

        [Fact]
        public async Task GetDetailsAsync_UpcomingTrip_ReturnsDaysUntilStart()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Croatia", 12, 4);

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(TripStatus.Upcoming, result.Status);
            Assert.Equal(12, result.DaysUntilStart);
        }

        [Fact]
        public async Task GetDetailsAsync_OngoingTrip_ReturnsCurrentDay()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Istanbul", -2, 5);

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(TripStatus.Ongoing, result.Status);
            Assert.Equal(3, result.CurrentDay);
        }

        [Fact]
        public async Task GetDetailsAsync_CompletedTrip_ReturnsDaysSinceEnd()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "New Zealand", -20, 14);

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(TripStatus.Completed, result.Status);
            Assert.Equal(6, result.DaysSinceEnd);
        }

        [Fact]
        public async Task GetDetailsAsync_ReturnsActiveDestinationsByNameAndNewestExpensesFirst()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Culture");
            var prague = await AddDestinationAsync(dbContext, category.Id, "Prague", "Czech Republic");
            var budapest = await AddDestinationAsync(dbContext, category.Id, "Budapest", "Hungary");
            var vienna = await AddDestinationAsync(dbContext, category.Id, "Vienna", "Austria");
            var trip = await AddTripAsync(dbContext, user.Id, "Central Europe", -30, 7, 1500m,
                prague.Id, budapest.Id, vienna.Id);
            vienna.IsDeleted = true;
            AddExpense(trip, "Flight", 124m, 0);
            AddExpense(trip, "Train", 39.90m, 3);
            AddExpense(trip, "Baths", 32m, 3);
            await dbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(new[] { "Budapest", "Prague" }, result.Destinations.Select(d => d.Name));
            Assert.Equal(new[] { "Baths", "Train", "Flight" }, result.Expenses.Select(e => e.Title));
        }

        [Fact]
        public async Task GetDetailsAsync_NoExpenses_ReturnsEmptyCategorySummary()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Iceland", 80, 8, 3200m);

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Empty(result.ExpensesByCategory);
        }

        [Fact]
        public async Task GetDetailsAsync_GroupsExpensesByCategoryFromHighestAmount()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Prague and Budapest", -60, 7, 1500m);
            AddExpense(trip, "Flight", 124m, 0, ExpenseCategory.Transport);
            AddExpense(trip, "Train", 39.90m, 3, ExpenseCategory.Transport);
            AddExpense(trip, "Hotel", 300m, 0, ExpenseCategory.Accommodation);
            AddExpense(trip, "Goulash", 20m, 4, ExpenseCategory.Food);
            AddExpense(trip, "Trdelnik", 16.10m, 1, ExpenseCategory.Food);
            await dbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            var summary = result.ExpensesByCategory.ToList();
            Assert.Equal(
                new[] { ExpenseCategory.Accommodation, ExpenseCategory.Transport, ExpenseCategory.Food },
                summary.Select(c => c.Category));
            Assert.Equal(new[] { 300m, 163.90m, 36.10m }, summary.Select(c => c.Amount));
            Assert.Equal(new[] { 1, 2, 2 }, summary.Select(c => c.Count));
            Assert.Equal(new[] { 60.0m, 32.8m, 7.2m }, summary.Select(c => c.Share));
        }

        [Fact]
        public async Task GetDetailsAsync_CategorySummary_AddsUpToTotalSpent()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Borovets", 40, 6, 800m);
            AddExpense(trip, "Ski pass", 250m, 0, ExpenseCategory.Activities);
            AddExpense(trip, "Bus", 18.50m, 0, ExpenseCategory.Transport);
            AddExpense(trip, "Dinner", 42.30m, 2, ExpenseCategory.Food);
            AddExpense(trip, "Souvenirs", 15m, 5, ExpenseCategory.Shopping);
            await dbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(result.TotalSpent, result.ExpensesByCategory.Sum(c => c.Amount));
            Assert.Equal(4, result.ExpensesByCategory.Count());
        }

        [Fact]
        public async Task GetDetailsAsync_SingleCategory_HasFullShare()
        {
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Velingrad", 10, 2, 300m);
            AddExpense(trip, "Spa hotel", 180m, 0, ExpenseCategory.Accommodation);
            AddExpense(trip, "Extra night", 63m, 1, ExpenseCategory.Accommodation);
            await dbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            var single = Assert.Single(result.ExpensesByCategory);
            Assert.Equal(243m, single.Amount);
            Assert.Equal(100m, single.Share);
        }

        [Fact]
        public async Task GetDetailsAsync_EqualAmounts_KeepCategoryOrderAndRoundShares()
        {
            var user = await AddUserAsync(dbContext, "ivan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Rila", -45, 1, 150m);
            AddExpense(trip, "Lunch", 10m, 0, ExpenseCategory.Food);
            AddExpense(trip, "Bus", 10m, 0, ExpenseCategory.Transport);
            AddExpense(trip, "Map", 10m, 1, ExpenseCategory.Other);
            await dbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(
                new[] { ExpenseCategory.Transport, ExpenseCategory.Food, ExpenseCategory.Other },
                result.ExpensesByCategory.Select(c => c.Category));
            Assert.All(result.ExpensesByCategory, c => Assert.Equal(33.3m, c.Share));
        }

        [Fact]
        public async Task GetForEditAsync_OwnTrip_ReturnsFormWithSelectedDestinations()
        {
            var user = await AddUserAsync(dbContext, "ivan@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Culture");
            var barcelona = await AddDestinationAsync(dbContext, category.Id, "Barcelona", "Spain");
            var trip = await AddTripAsync(dbContext, user.Id, "Food tour", 35, 5, 1300m, barcelona.Id);

            var result = await service.GetForEditAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal("Food tour", result.Title);
            Assert.Equal(1300m, result.Budget);
            Assert.Equal(new[] { barcelona.Id }, result.SelectedDestinationIds);
        }

        [Fact]
        public async Task GetForEditAsync_TripOfAnotherUser_ReturnsNull()
        {
            var owner = await AddUserAsync(dbContext, "ivan@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);

            Assert.Null(await service.GetForEditAsync(trip.Id, stranger.Id));
        }

        [Fact]
        public async Task EditAsync_OwnTrip_UpdatesFieldsAndSynchronizesDestinations()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Nature");
            var reykjavik = await AddDestinationAsync(dbContext, category.Id, "Reykjavik", "Iceland");
            var akureyri = await AddDestinationAsync(dbContext, category.Id, "Akureyri", "Iceland");
            var vik = await AddDestinationAsync(dbContext, category.Id, "Vik", "Iceland");
            var trip = await AddTripAsync(dbContext, user.Id, "Iceland", 80, 8, 3200m, reykjavik.Id, akureyri.Id);
            var start = DateTime.Today.AddDays(90);

            var result = await service.EditAsync(trip.Id, new TripFormViewModel
            {
                Title = " Iceland   Road Trip ",
                Description = " Waterfalls and hot springs ",
                StartDate = start,
                EndDate = start.AddDays(9),
                Budget = 3500m,
                CoverImageUrl = null,
                SelectedDestinationIds = new List<int> { reykjavik.Id, vik.Id, vik.Id }
            }, user.Id);

            Assert.True(result);
            var updated = await dbContext.Trips.Include(t => t.TripDestinations).SingleAsync();
            Assert.Equal("Iceland Road Trip", updated.Title);
            Assert.Equal("Waterfalls and hot springs", updated.Description);
            Assert.Equal(start, updated.StartDate);
            Assert.Equal(start.AddDays(9), updated.EndDate);
            Assert.Equal(3500m, updated.Budget);
            Assert.Equal(
                new[] { reykjavik.Id, vik.Id }.OrderBy(id => id),
                updated.TripDestinations.Select(td => td.DestinationId).OrderBy(id => id));
        }

        [Fact]
        public async Task EditAsync_TripOfAnotherUser_ReturnsFalseAndKeepsTrip()
        {
            var owner = await AddUserAsync(dbContext, "elena@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id, "Original title");

            var result = await service.EditAsync(trip.Id, CreateForm(), stranger.Id);

            Assert.False(result);
            Assert.Equal("Original title", (await dbContext.Trips.SingleAsync()).Title);
        }

        [Fact]
        public async Task EditAsync_MissingTrip_ReturnsFalse()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");

            Assert.False(await service.EditAsync(999, CreateForm(), user.Id));
        }

        [Fact]
        public async Task HasExpensesOutsideAsync_AllExpensesInsideTheDates_ReturnsFalse()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Sozopol", 10, 2);
            AddExpense(trip, "Room", 140m, 0);
            AddExpense(trip, "Boat trip", 25m, 2);
            await dbContext.SaveChangesAsync();

            Assert.False(await service.HasExpensesOutsideAsync(trip.Id, trip.StartDate, trip.EndDate));
        }

        [Fact]
        public async Task HasExpensesOutsideAsync_ExpenseBeforeNewStart_ReturnsTrue()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Sozopol", 10, 4);
            AddExpense(trip, "Bus ticket", 35m, 0);
            await dbContext.SaveChangesAsync();

            Assert.True(await service.HasExpensesOutsideAsync(trip.Id, trip.StartDate.AddDays(1), trip.EndDate));
        }

        [Fact]
        public async Task HasExpensesOutsideAsync_ExpenseAfterNewEnd_ReturnsTrue()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Sozopol", 10, 4);
            AddExpense(trip, "Fish restaurant", 58m, 4);
            await dbContext.SaveChangesAsync();

            Assert.True(await service.HasExpensesOutsideAsync(trip.Id, trip.StartDate, trip.EndDate.AddDays(-1)));
        }

        [Fact]
        public async Task HasExpensesOutsideAsync_IgnoresTheTimeOfTheNewDates()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Sozopol", 10, 2);
            AddExpense(trip, "Room", 140m, 0);
            await dbContext.SaveChangesAsync();

            var result = await service.HasExpensesOutsideAsync(
                trip.Id,
                trip.StartDate.AddHours(15),
                trip.EndDate.AddHours(8));

            Assert.False(result);
        }

        [Fact]
        public async Task HasExpensesOutsideAsync_OtherTripsAreIgnored()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Sozopol", 10, 2);
            var otherTrip = await AddTripAsync(dbContext, user.Id, "Kyoto", 100, 12);
            AddExpense(otherTrip, "Flights", 1180m, 0);
            await dbContext.SaveChangesAsync();

            Assert.False(await service.HasExpensesOutsideAsync(trip.Id, trip.StartDate, trip.EndDate));
        }

        [Fact]
        public async Task DeleteAsync_OwnTrip_RemovesTripWithItsExpensesAndPackingItems()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Borovets");
            var otherTrip = await AddTripAsync(dbContext, user.Id, "New Zealand");
            AddExpense(trip, "Lift pass", 312m, 0);
            dbContext.PackingItems.Add(new PackingItem
            {
                TripId = trip.Id,
                Name = "Ski goggles",
                Category = PackingCategory.Other
            });
            await dbContext.SaveChangesAsync();

            var result = await service.DeleteAsync(trip.Id, user.Id);

            Assert.True(result);
            Assert.Equal("New Zealand", (await dbContext.Trips.SingleAsync()).Title);
            Assert.Empty(dbContext.Expenses);
            Assert.Empty(dbContext.PackingItems);
        }

        [Fact]
        public async Task DeleteAsync_TripOfAnotherUser_ReturnsFalseAndKeepsTrip()
        {
            var owner = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);

            var result = await service.DeleteAsync(trip.Id, stranger.Id);

            Assert.False(result);
            Assert.Single(dbContext.Trips);
        }

        [Fact]
        public async Task DeleteAsync_MissingTrip_ReturnsFalse()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");

            Assert.False(await service.DeleteAsync(999, user.Id));
        }

        private void AddExpense(
            Trip trip,
            string title,
            decimal amount,
            int dayOfTrip,
            ExpenseCategory category = ExpenseCategory.Other)
        {
            dbContext.Expenses.Add(new Expense
            {
                TripId = trip.Id,
                Title = title,
                Amount = amount,
                Category = category,
                Date = trip.StartDate.AddDays(dayOfTrip)
            });
        }

        private static TripFormViewModel CreateForm()
        {
            var start = DateTime.Today.AddDays(10);

            return new TripFormViewModel
            {
                Title = "Changed title",
                StartDate = start,
                EndDate = start.AddDays(2),
                Budget = 500m,
                SelectedDestinationIds = new List<int>()
            };
        }
    }
}
