using Voyage.Common.Constants;
using Voyage.Data;
using Voyage.Services.Tests.Infrastructure;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class HomeServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly HomeService service;

        public HomeServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new HomeService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetIndexAsync_EmptyDatabase_ReturnsZeroCountsAndNoDestinations()
        {
            var result = await service.GetIndexAsync();

            Assert.Equal(0, result.DestinationsCount);
            Assert.Equal(0, result.CategoriesCount);
            Assert.Equal(0, result.TripsCount);
            Assert.Empty(result.PopularDestinations);
        }

        [Fact]
        public async Task GetIndexAsync_ReturnsCountsOfDestinationsCategoriesAndTrips()
        {
            var beach = await AddCategoryAsync(dbContext, "Beach");
            var city = await AddCategoryAsync(dbContext, "City Break");
            await AddDestinationAsync(dbContext, beach.Id, "Sozopol");
            await AddDestinationAsync(dbContext, beach.Id, "Nessebar");
            await AddDestinationAsync(dbContext, city.Id, "Plovdiv");
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var ivan = await AddUserAsync(dbContext, "ivan@voyage.test");
            await AddTripAsync(dbContext, maria.Id, "Summer on the coast");
            await AddTripAsync(dbContext, ivan.Id, "Weekend in Plovdiv");

            var result = await service.GetIndexAsync();

            Assert.Equal(3, result.DestinationsCount);
            Assert.Equal(2, result.CategoriesCount);
            Assert.Equal(2, result.TripsCount);
        }

        [Fact]
        public async Task GetIndexAsync_DeletedDestinations_AreNotCounted()
        {
            var category = await AddCategoryAsync(dbContext, "Mountains");
            await AddDestinationAsync(dbContext, category.Id, "Bansko");
            await AddDestinationAsync(dbContext, category.Id, "Old ski hut", isDeleted: true);

            var result = await service.GetIndexAsync();

            Assert.Equal(1, result.DestinationsCount);
            Assert.DoesNotContain(result.PopularDestinations, d => d.Name == "Old ski hut");
        }

        [Fact]
        public async Task GetIndexAsync_PopularDestinations_AreOrderedByNumberOfTrips()
        {
            var category = await AddCategoryAsync(dbContext, "City Break");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");
            var vienna = await AddDestinationAsync(dbContext, category.Id, "Vienna", "Austria");
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            await AddTripAsync(dbContext, user.Id, "Rome and Paris", destinationIds: new[] { rome.Id, paris.Id });
            await AddTripAsync(dbContext, user.Id, "Rome again", destinationIds: new[] { rome.Id });
            await AddTripAsync(dbContext, user.Id, "Rome with friends", destinationIds: new[] { rome.Id });
            await AddTripAsync(dbContext, user.Id, "Paris and Vienna", destinationIds: new[] { paris.Id, vienna.Id });

            var result = await service.GetIndexAsync();

            Assert.Equal(new[] { "Rome", "Paris", "Vienna" }, result.PopularDestinations.Select(d => d.Name));
        }

        [Fact]
        public async Task GetIndexAsync_SameNumberOfTrips_OrdersByName()
        {
            var category = await AddCategoryAsync(dbContext, "Nature");
            await AddDestinationAsync(dbContext, category.Id, "Seven Rila Lakes");
            await AddDestinationAsync(dbContext, category.Id, "Belogradchik Rocks");
            await AddDestinationAsync(dbContext, category.Id, "Krushuna Falls");

            var result = await service.GetIndexAsync();

            Assert.Equal(
                new[] { "Belogradchik Rocks", "Krushuna Falls", "Seven Rila Lakes" },
                result.PopularDestinations.Select(d => d.Name));
        }

        [Fact]
        public async Task GetIndexAsync_ManyDestinations_ReturnsOnlyTheConfiguredCount()
        {
            var category = await AddCategoryAsync(dbContext, "Beach");
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");

            for (int i = 1; i <= PagingConstants.PopularDestinationsCount + 3; i++)
            {
                var destination = await AddDestinationAsync(dbContext, category.Id, $"Beach {i}");

                for (int t = 0; t < i; t++)
                {
                    await AddTripAsync(dbContext, user.Id, $"Trip {i}-{t}", destinationIds: new[] { destination.Id });
                }
            }

            var result = await service.GetIndexAsync();

            Assert.Equal(PagingConstants.PopularDestinationsCount, result.PopularDestinations.Count());
            Assert.Equal("Beach 6", result.PopularDestinations.First().Name);
            Assert.Equal(6, result.DestinationsCount);
        }

        [Fact]
        public async Task GetIndexAsync_PopularDestination_ContainsCardDetails()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Veliko Tarnovo");

            var result = await service.GetIndexAsync();

            var card = Assert.Single(result.PopularDestinations);
            Assert.Equal(destination.Id, card.Id);
            Assert.Equal("Veliko Tarnovo", card.Name);
            Assert.Equal("Bulgaria", card.Country);
            Assert.Equal("Culture", card.CategoryName);
            Assert.Equal(destination.Description, card.Description);
            Assert.False(card.IsFavorite);
        }

        [Fact]
        public async Task GetIndexAsync_PopularDestination_RatingIgnoresHiddenReviews()
        {
            var category = await AddCategoryAsync(dbContext, "City Break");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var troll = await AddUserAsync(dbContext, "troll@voyage.test");
            await AddReviewAsync(dbContext, maria.Id, destination.Id, 5);
            await AddReviewAsync(dbContext, georgi.Id, destination.Id, 4);
            await AddReviewAsync(dbContext, troll.Id, destination.Id, 1, isHidden: true);

            var result = await service.GetIndexAsync();

            var card = Assert.Single(result.PopularDestinations);
            Assert.Equal(2, card.ReviewsCount);
            Assert.Equal(4.5, card.AverageRating);
        }

        [Fact]
        public async Task GetIndexAsync_DestinationWithoutReviews_HasNoRating()
        {
            var category = await AddCategoryAsync(dbContext, "Nature");
            await AddDestinationAsync(dbContext, category.Id, "Pirin");

            var result = await service.GetIndexAsync();

            var card = Assert.Single(result.PopularDestinations);
            Assert.Equal(0, card.ReviewsCount);
            Assert.Null(card.AverageRating);
        }
    }
}
