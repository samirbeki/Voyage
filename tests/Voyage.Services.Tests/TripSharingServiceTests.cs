using Microsoft.EntityFrameworkCore;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Data.Models.Enums;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Enums;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class TripSharingServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly TripSharingService service;

        public TripSharingServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new TripSharingService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task ShareAsync_OwnTrip_CreatesToken()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Summer in Italy");

            var token = await service.ShareAsync(trip.Id, user.Id);

            Assert.NotNull(token);
            Assert.NotEqual(Guid.Empty, token.Value);
            Assert.Equal(token, (await dbContext.Trips.AsNoTracking().SingleAsync()).ShareToken);
        }

        [Fact]
        public async Task ShareAsync_AlreadyShared_KeepsTheSameToken()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            var first = await service.ShareAsync(trip.Id, user.Id);
            var second = await service.ShareAsync(trip.Id, user.Id);

            Assert.Equal(first, second);
        }

        [Fact]
        public async Task ShareAsync_TripsGetDifferentTokens()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var rome = await AddTripAsync(dbContext, user.Id, "Rome");
            var paris = await AddTripAsync(dbContext, user.Id, "Paris");

            var romeToken = await service.ShareAsync(rome.Id, user.Id);
            var parisToken = await service.ShareAsync(paris.Id, user.Id);

            Assert.NotEqual(romeToken, parisToken);
        }

        [Fact]
        public async Task ShareAsync_TripOfAnotherUser_ReturnsNullAndDoesNotShare()
        {
            var owner = await AddUserAsync(dbContext, "maria@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);

            Assert.Null(await service.ShareAsync(trip.Id, stranger.Id));
            Assert.Null((await dbContext.Trips.AsNoTracking().SingleAsync()).ShareToken);
        }

        [Fact]
        public async Task ShareAsync_MissingTrip_ReturnsNull()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");

            Assert.Null(await service.ShareAsync(999, user.Id));
        }

        [Fact]
        public async Task RegenerateAsync_SharedTrip_CreatesNewTokenAndOldOneStopsWorking()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var oldToken = await service.ShareAsync(trip.Id, user.Id);

            var newToken = await service.RegenerateAsync(trip.Id, user.Id);

            Assert.NotNull(newToken);
            Assert.NotEqual(oldToken, newToken);
            Assert.Null(await service.GetSharedAsync(oldToken!.Value));
            Assert.NotNull(await service.GetSharedAsync(newToken.Value));
        }

        [Fact]
        public async Task RegenerateAsync_TripOfAnotherUser_ReturnsNullAndKeepsToken()
        {
            var owner = await AddUserAsync(dbContext, "maria@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var token = await service.ShareAsync(trip.Id, owner.Id);

            Assert.Null(await service.RegenerateAsync(trip.Id, stranger.Id));
            Assert.NotNull(await service.GetSharedAsync(token!.Value));
        }

        [Fact]
        public async Task StopSharingAsync_SharedTrip_RemovesToken()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var token = await service.ShareAsync(trip.Id, user.Id);

            var result = await service.StopSharingAsync(trip.Id, user.Id);

            Assert.True(result);
            Assert.Null((await dbContext.Trips.AsNoTracking().SingleAsync()).ShareToken);
            Assert.Null(await service.GetSharedAsync(token!.Value));
        }

        [Fact]
        public async Task StopSharingAsync_TripThatIsNotShared_ReturnsTrue()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            Assert.True(await service.StopSharingAsync(trip.Id, user.Id));
        }

        [Fact]
        public async Task StopSharingAsync_TripOfAnotherUser_ReturnsFalseAndKeepsSharing()
        {
            var owner = await AddUserAsync(dbContext, "maria@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var token = await service.ShareAsync(trip.Id, owner.Id);

            Assert.False(await service.StopSharingAsync(trip.Id, stranger.Id));
            Assert.NotNull(await service.GetSharedAsync(token!.Value));
        }

        [Fact]
        public async Task StopThenShareAgain_CreatesNewToken()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var first = await service.ShareAsync(trip.Id, user.Id);

            await service.StopSharingAsync(trip.Id, user.Id);
            var second = await service.ShareAsync(trip.Id, user.Id);

            Assert.NotNull(second);
            Assert.NotEqual(first, second);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task SharingMethods_EmptyUserId_Throw(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.ShareAsync(1, userId!));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.RegenerateAsync(1, userId!));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.StopSharingAsync(1, userId!));
        }

        [Fact]
        public async Task GetSharedAsync_ReturnsReadOnlyTripDetails()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var category = await AddCategoryAsync(dbContext, "City Break");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var florence = await AddDestinationAsync(dbContext, category.Id, "Florence", "Italy");
            var trip = await AddTripAsync(dbContext, user.Id, "Summer in Italy", 15, 6, 1800m, rome.Id, florence.Id);
            trip.Description = "A week of food, history and long walks.";
            await dbContext.SaveChangesAsync();
            var token = await service.ShareAsync(trip.Id, user.Id);

            var result = await service.GetSharedAsync(token!.Value);

            Assert.NotNull(result);
            Assert.Equal("Summer in Italy", result.Title);
            Assert.Equal("A week of food, history and long walks.", result.Description);
            Assert.Equal("maria", result.OwnerName);
            Assert.Equal(trip.StartDate, result.StartDate);
            Assert.Equal(7, result.DaysCount);
            Assert.Equal(TripStatus.Upcoming, result.Status);
            Assert.Equal(new[] { "Florence", "Rome" }, result.Destinations.Select(d => d.Name));
        }

        [Fact]
        public async Task GetSharedAsync_SummarizesExpensesByCategory()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Prague", -30, 3, 900m);
            await AddExpenseAsync(trip, 300m, ExpenseCategory.Accommodation);
            await AddExpenseAsync(trip, 60m, ExpenseCategory.Food);
            await AddExpenseAsync(trip, 40m, ExpenseCategory.Food);
            var token = await service.ShareAsync(trip.Id, user.Id);

            var result = await service.GetSharedAsync(token!.Value);

            Assert.NotNull(result);
            Assert.Equal(400m, result.TotalSpent);
            Assert.Equal(
                new[] { ExpenseCategory.Accommodation, ExpenseCategory.Food },
                result.ExpensesByCategory.Select(c => c.Category));
            Assert.Equal(new[] { 75m, 25m }, result.ExpensesByCategory.Select(c => c.Share));
            Assert.Equal(TripStatus.Completed, result.Status);
        }

        [Fact]
        public async Task GetSharedAsync_OngoingTrip_HasOngoingStatus()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Istanbul Food Week", -2, 5);
            var token = await service.ShareAsync(trip.Id, user.Id);

            var result = await service.GetSharedAsync(token!.Value);

            Assert.Equal(TripStatus.Ongoing, result!.Status);
        }

        [Fact]
        public async Task GetSharedAsync_DeletedDestination_IsNotShown()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Nature");
            var plitvice = await AddDestinationAsync(dbContext, category.Id, "Plitvice Lakes", "Croatia");
            var closed = await AddDestinationAsync(dbContext, category.Id, "Closed park", "Croatia");
            var trip = await AddTripAsync(dbContext, user.Id, "Croatia", 20, 3, 700m, plitvice.Id, closed.Id);
            closed.IsDeleted = true;
            await dbContext.SaveChangesAsync();
            var token = await service.ShareAsync(trip.Id, user.Id);

            var result = await service.GetSharedAsync(token!.Value);

            Assert.Equal("Plitvice Lakes", Assert.Single(result!.Destinations).Name);
        }

        [Fact]
        public async Task GetSharedAsync_UnknownOrEmptyToken_ReturnsNull()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await service.ShareAsync(trip.Id, user.Id);

            Assert.Null(await service.GetSharedAsync(Guid.NewGuid()));
            Assert.Null(await service.GetSharedAsync(Guid.Empty));
        }

        [Fact]
        public async Task GetSharedAsync_TripThatWasNeverShared_IsNotReachable()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddTripAsync(dbContext, user.Id, "Private trip");

            Assert.Null(await service.GetSharedAsync(Guid.Empty));
            Assert.All(await dbContext.Trips.ToListAsync(), t => Assert.Null(t.ShareToken));
        }

        [Fact]
        public async Task TripServiceGetDetails_ReturnsCurrentShareToken()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var token = await service.ShareAsync(trip.Id, user.Id);
            var tripService = new TripService(dbContext);

            var details = await tripService.GetDetailsAsync(trip.Id, user.Id);

            Assert.Equal(token, details!.ShareToken);
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
    }
}
