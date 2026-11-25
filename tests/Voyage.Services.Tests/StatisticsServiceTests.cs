using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Data.Models.Enums;
using Voyage.Services.Tests.Infrastructure;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class StatisticsServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly StatisticsService service;

        public StatisticsServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new StatisticsService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetForUserAsync_UserWithoutAnything_ReturnsEmptyStatistics()
        {
            var user = await AddUserAsync(dbContext, "new.user@voyage.test");

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(0, result.TripsCount);
            Assert.Equal(0, result.DaysTraveled);
            Assert.Equal(0, result.CountriesVisited);
            Assert.Equal(0m, result.TotalSpent);
            Assert.Equal(0, result.BudgetUsedPercent);
            Assert.Equal(0m, result.AverageSpentPerDay);
            Assert.Empty(result.ExpensesByCategory);
            Assert.Empty(result.TopCountries);
            Assert.Null(result.NextTrip);
            Assert.Null(result.MostExpensiveTrip);
            Assert.Equal(0, result.ReviewsCount);
            Assert.Null(result.AverageRatingGiven);
            Assert.Equal(0, result.FavoritesCount);
        }

        [Fact]
        public async Task GetForUserAsync_CountsTripsByStatus()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            await AddTripAsync(dbContext, user.Id, "Croatia", 10, 4);
            await AddTripAsync(dbContext, user.Id, "Istanbul", -1, 3);
            await AddTripAsync(dbContext, user.Id, "Bansko", -30, 2);
            await AddTripAsync(dbContext, user.Id, "Prague", -60, 4);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(4, result.TripsCount);
            Assert.Equal(1, result.UpcomingCount);
            Assert.Equal(1, result.OngoingCount);
            Assert.Equal(2, result.CompletedCount);
        }

        [Fact]
        public async Task GetForUserAsync_DaysTraveled_CountsCompletedTripsAndPassedDaysOfOngoingTrip()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            await AddTripAsync(dbContext, user.Id, "Rome", -30, 4);
            await AddTripAsync(dbContext, user.Id, "Istanbul", -2, 5);
            await AddTripAsync(dbContext, user.Id, "Kyoto", 40, 10);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(5 + 3, result.DaysTraveled);
        }

        [Fact]
        public async Task GetForUserAsync_VisitedPlaces_CountOnlyStartedTripsAndEachPlaceOnce()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var category = await AddCategoryAsync(dbContext, "City Break");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var florence = await AddDestinationAsync(dbContext, category.Id, "Florence", "Italy");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");
            var kyoto = await AddDestinationAsync(dbContext, category.Id, "Kyoto", "Japan");
            await AddTripAsync(dbContext, user.Id, "Italy", -40, 6, 1500m, rome.Id, florence.Id);
            await AddTripAsync(dbContext, user.Id, "Paris", -20, 3, 900m, paris.Id);
            await AddTripAsync(dbContext, user.Id, "Paris again", -1, 2, 600m, paris.Id);
            await AddTripAsync(dbContext, user.Id, "Japan", 30, 10, 3500m, kyoto.Id);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(3, result.DestinationsVisited);
            Assert.Equal(2, result.CountriesVisited);
        }

        [Fact]
        public async Task GetForUserAsync_DeletedDestination_IsNotCounted()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Nature");
            var plitvice = await AddDestinationAsync(dbContext, category.Id, "Plitvice Lakes", "Croatia");
            var oldCamp = await AddDestinationAsync(dbContext, category.Id, "Old camp", "Slovenia");
            await AddTripAsync(dbContext, user.Id, "Lakes", -50, 3, 700m, plitvice.Id, oldCamp.Id);
            oldCamp.IsDeleted = true;
            await dbContext.SaveChangesAsync();

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(1, result.DestinationsVisited);
            Assert.Equal(1, result.CountriesVisited);
            Assert.Equal("Croatia", Assert.Single(result.TopCountries).Country);
        }

        [Fact]
        public async Task GetForUserAsync_BudgetOverview_SumsAllTrips()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var paris = await AddTripAsync(dbContext, user.Id, "Paris", -15, 4, 1000m);
            var vienna = await AddTripAsync(dbContext, user.Id, "Vienna", -90, 3, 500m);
            await AddExpenseAsync(paris, 600m, ExpenseCategory.Accommodation);
            await AddExpenseAsync(vienna, 700m, ExpenseCategory.Food);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(1500m, result.TotalBudget);
            Assert.Equal(1300m, result.TotalSpent);
            Assert.Equal(86, result.BudgetUsedPercent);
            Assert.Equal(1, result.TripsOverBudget);
        }

        [Fact]
        public async Task GetForUserAsync_SpentMoreThanAllBudgets_CapsPercentAtHundred()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Santorini", -10, 4, 800m);
            await AddExpenseAsync(trip, 950m, ExpenseCategory.Accommodation);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(100, result.BudgetUsedPercent);
            Assert.Equal(1, result.TripsOverBudget);
        }

        [Fact]
        public async Task GetForUserAsync_AverageSpentPerDay_UsesOnlyTripsWithExpenses()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var rila = await AddTripAsync(dbContext, user.Id, "Rila", -100, 4, 600m);
            var plovdiv = await AddTripAsync(dbContext, user.Id, "Plovdiv", -50, 1, 300m);
            await AddTripAsync(dbContext, user.Id, "Not planned yet", 60, 9, 2000m);
            await AddExpenseAsync(rila, 500m, ExpenseCategory.Accommodation);
            await AddExpenseAsync(plovdiv, 100m, ExpenseCategory.Food);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(85.71m, result.AverageSpentPerDay);
        }

        [Fact]
        public async Task GetForUserAsync_ExpensesByCategory_CombinesAllTrips()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var prague = await AddTripAsync(dbContext, user.Id, "Prague", -60, 4, 1200m);
            var budapest = await AddTripAsync(dbContext, user.Id, "Budapest", -30, 3, 900m);
            await AddExpenseAsync(prague, 300m, ExpenseCategory.Accommodation);
            await AddExpenseAsync(prague, 80m, ExpenseCategory.Food);
            await AddExpenseAsync(budapest, 250m, ExpenseCategory.Accommodation);
            await AddExpenseAsync(budapest, 70m, ExpenseCategory.Food);
            await AddExpenseAsync(budapest, 100m, ExpenseCategory.Transport);

            var result = await service.GetForUserAsync(user.Id);

            var summary = result.ExpensesByCategory.ToList();
            Assert.Equal(
                new[] { ExpenseCategory.Accommodation, ExpenseCategory.Food, ExpenseCategory.Transport },
                summary.Select(c => c.Category));
            Assert.Equal(new[] { 550m, 150m, 100m }, summary.Select(c => c.Amount));
            Assert.Equal(new[] { 2, 2, 1 }, summary.Select(c => c.Count));
            Assert.Equal(new[] { 68.8m, 18.8m, 12.5m }, summary.Select(c => c.Share));
        }

        [Fact]
        public async Task GetForUserAsync_TopCountries_OrderedByTripsAndLimitedToFive()
        {
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var venice = await AddDestinationAsync(dbContext, category.Id, "Venice", "Italy");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");
            var lisbon = await AddDestinationAsync(dbContext, category.Id, "Lisbon", "Portugal");
            var prague = await AddDestinationAsync(dbContext, category.Id, "Prague", "Czech Republic");
            var vienna = await AddDestinationAsync(dbContext, category.Id, "Vienna", "Austria");
            var berlin = await AddDestinationAsync(dbContext, category.Id, "Berlin", "Germany");
            await AddTripAsync(dbContext, user.Id, "Rome and Venice", -200, 6, 1500m, rome.Id, venice.Id);
            await AddTripAsync(dbContext, user.Id, "Rome and Paris", -150, 6, 1500m, rome.Id, paris.Id);
            await AddTripAsync(dbContext, user.Id, "Venice", -100, 3, 800m, venice.Id);
            await AddTripAsync(dbContext, user.Id, "Paris", -80, 3, 900m, paris.Id);
            await AddTripAsync(dbContext, user.Id, "Lisbon", -60, 4, 900m, lisbon.Id);
            await AddTripAsync(dbContext, user.Id, "Prague", -40, 3, 600m, prague.Id);
            await AddTripAsync(dbContext, user.Id, "Vienna and Berlin", 20, 6, 1400m, vienna.Id, berlin.Id);

            var result = await service.GetForUserAsync(user.Id);

            var countries = result.TopCountries.ToList();
            Assert.Equal(
                new[] { "Italy", "France", "Austria", "Czech Republic", "Germany" },
                countries.Select(c => c.Country));
            Assert.Equal(new[] { 3, 2, 1, 1, 1 }, countries.Select(c => c.TripsCount));
            Assert.Equal(43, countries[0].Percent);
        }

        [Fact]
        public async Task GetForUserAsync_NextTrip_IsTheClosestUpcomingTrip()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            await AddTripAsync(dbContext, user.Id, "Iceland", 90, 8);
            await AddTripAsync(dbContext, user.Id, "Borovets", 12, 5);
            await AddTripAsync(dbContext, user.Id, "Thessaloniki", -20, 3);

            var result = await service.GetForUserAsync(user.Id);

            Assert.NotNull(result.NextTrip);
            Assert.Equal("Borovets", result.NextTrip.Title);
            Assert.Equal(12, result.NextTrip.DaysUntilStart);
        }

        [Fact]
        public async Task GetForUserAsync_OnlyPastTrips_HasNoNextTrip()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            await AddTripAsync(dbContext, user.Id, "Thessaloniki", -20, 3);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Null(result.NextTrip);
        }

        [Fact]
        public async Task GetForUserAsync_MostExpensiveTrip_HasTheHighestSpending()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var krakow = await AddTripAsync(dbContext, user.Id, "Krakow", -70, 4, 900m);
            var newZealand = await AddTripAsync(dbContext, user.Id, "New Zealand", -200, 14, 6000m);
            await AddTripAsync(dbContext, user.Id, "Empty plan", 30, 2, 400m);
            await AddExpenseAsync(krakow, 420m, ExpenseCategory.Accommodation);
            await AddExpenseAsync(newZealand, 2100m, ExpenseCategory.Transport);
            await AddExpenseAsync(newZealand, 1500m, ExpenseCategory.Accommodation);

            var result = await service.GetForUserAsync(user.Id);

            Assert.NotNull(result.MostExpensiveTrip);
            Assert.Equal(newZealand.Id, result.MostExpensiveTrip.Id);
            Assert.Equal(3600m, result.MostExpensiveTrip.Spent);
        }

        [Fact]
        public async Task GetForUserAsync_IgnoresTripsOfOtherUsers()
        {
            var user = await AddUserAsync(dbContext, "ivan@voyage.test");
            var other = await AddUserAsync(dbContext, "maria@voyage.test");
            var own = await AddTripAsync(dbContext, user.Id, "Sofia", -5, 1, 200m);
            var foreign = await AddTripAsync(dbContext, other.Id, "Barcelona", -5, 4, 1400m);
            await AddExpenseAsync(own, 50m, ExpenseCategory.Food);
            await AddExpenseAsync(foreign, 900m, ExpenseCategory.Accommodation);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(1, result.TripsCount);
            Assert.Equal(200m, result.TotalBudget);
            Assert.Equal(50m, result.TotalSpent);
            Assert.Equal(own.Id, result.MostExpensiveTrip!.Id);
        }

        [Fact]
        public async Task GetForUserAsync_Reviews_CountsOwnActiveReviewsAndAverageRating()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var other = await AddUserAsync(dbContext, "elena@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Mountains");
            var bansko = await AddDestinationAsync(dbContext, category.Id, "Bansko");
            var borovets = await AddDestinationAsync(dbContext, category.Id, "Borovets");
            var pamporovo = await AddDestinationAsync(dbContext, category.Id, "Pamporovo");
            var closed = await AddDestinationAsync(dbContext, category.Id, "Closed resort", isDeleted: true);
            var rila = await AddDestinationAsync(dbContext, category.Id, "Rila");
            await AddReviewAsync(dbContext, user.Id, bansko.Id, 5);
            await AddReviewAsync(dbContext, user.Id, borovets.Id, 4, isHidden: true);
            await AddReviewAsync(dbContext, user.Id, pamporovo.Id, 4);
            await AddReviewAsync(dbContext, user.Id, rila.Id, 1, isDeleted: true);
            await AddReviewAsync(dbContext, user.Id, closed.Id, 1);
            await AddReviewAsync(dbContext, other.Id, bansko.Id, 1);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(3, result.ReviewsCount);
            Assert.Equal(4.3, result.AverageRatingGiven);
        }

        [Fact]
        public async Task GetForUserAsync_Favorites_CountsOnlyOwnActiveDestinations()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var other = await AddUserAsync(dbContext, "petya@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Beach");
            var sozopol = await AddDestinationAsync(dbContext, category.Id, "Sozopol");
            var santorini = await AddDestinationAsync(dbContext, category.Id, "Santorini", "Greece");
            var closed = await AddDestinationAsync(dbContext, category.Id, "Closed beach", isDeleted: true);
            await AddFavoriteAsync(user.Id, sozopol.Id);
            await AddFavoriteAsync(user.Id, santorini.Id);
            await AddFavoriteAsync(user.Id, closed.Id);
            await AddFavoriteAsync(other.Id, sozopol.Id);

            var result = await service.GetForUserAsync(user.Id);

            Assert.Equal(2, result.FavoritesCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetForUserAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.GetForUserAsync(userId!));
        }

        private async Task AddExpenseAsync(Trip trip, decimal amount, ExpenseCategory category)
        {
            dbContext.Expenses.Add(new Expense
            {
                TripId = trip.Id,
                Title = $"{category} expense",
                Amount = amount,
                Category = category,
                Date = trip.StartDate
            });

            await dbContext.SaveChangesAsync();
        }

        private async Task AddFavoriteAsync(string userId, int destinationId)
        {
            dbContext.Favorites.Add(new Favorite
            {
                UserId = userId,
                DestinationId = destinationId,
                AddedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }
    }
}
