using Voyage.Common.Constants;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Services.Contracts;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Weather;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class TripWeatherServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly FakeWeatherService weatherService = new();
        private readonly TripWeatherService service;

        public TripWeatherServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new TripWeatherService(dbContext, weatherService);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetForTripAsync_TripWithinTheForecast_ReturnsOnlyTheTripDays()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var lisbon = await AddLisbonAsync();
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: 2, lengthInDays: 3, destinationIds: [lisbon.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.False(result.IsFinished);
            Assert.False(result.IsTooEarly);
            Assert.False(result.IsPartial);
            Assert.Equal(DateTime.Today.AddDays(5), result.CoveredUntil);

            var destination = Assert.Single(result.Destinations);
            Assert.Equal(lisbon.Id, destination.DestinationId);
            Assert.Equal("Lisbon", destination.Name);
            Assert.Equal("Portugal", destination.Country);
            Assert.Equal(
                new[] { 2, 3, 4, 5 }.Select(d => DateTime.Today.AddDays(d)),
                destination.Weather!.Days.Select(d => d.Date));
        }

        [Fact]
        public async Task GetForTripAsync_OngoingTrip_StartsFromToday()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var lisbon = await AddLisbonAsync();
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: -2, lengthInDays: 4, destinationIds: [lisbon.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            var days = Assert.Single(result!.Destinations).Weather!.Days.ToList();
            Assert.Equal(DateTime.Today, days.First().Date);
            Assert.Equal(DateTime.Today.AddDays(2), days.Last().Date);
        }

        [Fact]
        public async Task GetForTripAsync_TripEndsAfterTheForecast_IsPartial()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var lisbon = await AddLisbonAsync();
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: 10, lengthInDays: 10, destinationIds: [lisbon.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            var lastForecastDay = DateTime.Today.AddDays(WeatherConstants.MaxForecastDays - 1);
            Assert.True(result!.IsPartial);
            Assert.Equal(lastForecastDay, result.CoveredUntil);

            var days = Assert.Single(result.Destinations).Weather!.Days.ToList();
            Assert.Equal(DateTime.Today.AddDays(10), days.First().Date);
            Assert.Equal(lastForecastDay, days.Last().Date);
        }

        [Fact]
        public async Task GetForTripAsync_TripStartsOnTheLastForecastDay_ShowsThatDay()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var lisbon = await AddLisbonAsync();
            var trip = await AddTripAsync(
                dbContext,
                user.Id,
                startInDays: WeatherConstants.MaxForecastDays - 1,
                lengthInDays: 3,
                destinationIds: [lisbon.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            Assert.False(result!.IsTooEarly);
            Assert.True(result.IsPartial);
            Assert.Single(Assert.Single(result.Destinations).Weather!.Days);
        }

        [Fact]
        public async Task GetForTripAsync_TripTooFarAway_IsTooEarlyWithoutAskingForWeather()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var lisbon = await AddLisbonAsync();
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: 30, lengthInDays: 5, destinationIds: [lisbon.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            Assert.True(result!.IsTooEarly);
            Assert.False(result.IsPartial);
            Assert.Equal(DateTime.Today.AddDays(30 - (WeatherConstants.MaxForecastDays - 1)), result.AvailableFrom);
            Assert.Empty(result.Destinations);
            Assert.Empty(weatherService.Requests);
        }

        [Fact]
        public async Task GetForTripAsync_FinishedTrip_IsFinishedWithoutAskingForWeather()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var lisbon = await AddLisbonAsync();
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: -10, lengthInDays: 3, destinationIds: [lisbon.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            Assert.True(result!.IsFinished);
            Assert.False(result.IsPartial);
            Assert.Empty(result.Destinations);
            Assert.Empty(weatherService.Requests);
        }

        [Fact]
        public async Task GetForTripAsync_SeveralDestinations_AsksForEachOrderedByName()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var category = await AddCategoryAsync(dbContext, "City Break");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: 1, lengthInDays: 4, destinationIds: [rome.Id, paris.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            Assert.Equal(new[] { "Paris", "Rome" }, result!.Destinations.Select(d => d.Name));
            Assert.Equal(new[] { "Paris|France", "Rome|Italy" }, weatherService.Requests);
        }

        [Fact]
        public async Task GetForTripAsync_WeatherUnavailableForOneDestination_KeepsTheOthers()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Nature");
            var atlantis = await AddDestinationAsync(dbContext, category.Id, "Atlantis", "Nowhere");
            var bansko = await AddDestinationAsync(dbContext, category.Id, "Bansko");
            weatherService.Unavailable.Add("Atlantis");
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: 1, lengthInDays: 2, destinationIds: [atlantis.Id, bansko.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            var destinations = result!.Destinations.ToList();
            Assert.Null(destinations[0].Weather);
            Assert.NotNull(destinations[1].Weather);
        }

        [Fact]
        public async Task GetForTripAsync_ForecastHasNoDaysForTheTrip_WeatherIsNull()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var lisbon = await AddLisbonAsync();
            weatherService.ForecastLength = 2;
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: 5, lengthInDays: 2, destinationIds: [lisbon.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            Assert.Null(Assert.Single(result!.Destinations).Weather);
        }

        [Fact]
        public async Task GetForTripAsync_DeletedDestination_IsSkipped()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Nature");
            var deleted = await AddDestinationAsync(dbContext, category.Id, "Old place", isDeleted: true);
            var bansko = await AddDestinationAsync(dbContext, category.Id, "Bansko");
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: 1, lengthInDays: 2, destinationIds: [deleted.Id, bansko.Id]);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            Assert.Equal("Bansko", Assert.Single(result!.Destinations).Name);
            Assert.Equal(new[] { "Bansko|Bulgaria" }, weatherService.Requests);
        }

        [Fact]
        public async Task GetForTripAsync_TripWithoutDestinations_ReturnsNoDestinations()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: 1);

            var result = await service.GetForTripAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Empty(result.Destinations);
        }

        [Fact]
        public async Task GetForTripAsync_TripOfAnotherUser_ReturnsNull()
        {
            var owner = await AddUserAsync(dbContext, "ana@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id, startInDays: 1);

            var result = await service.GetForTripAsync(trip.Id, stranger.Id);

            Assert.Null(result);
            Assert.Empty(weatherService.Requests);
        }

        [Fact]
        public async Task GetForTripAsync_MissingTrip_ReturnsNull()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");

            var result = await service.GetForTripAsync(999, user.Id);

            Assert.Null(result);
        }

        private async Task<Destination> AddLisbonAsync()
        {
            var category = await AddCategoryAsync(dbContext, "City Break");

            return await AddDestinationAsync(dbContext, category.Id, "Lisbon", "Portugal");
        }

        private sealed class FakeWeatherService : IWeatherService
        {
            public List<string> Requests { get; } = new();

            public List<string> Unavailable { get; } = new();

            public int ForecastLength { get; set; } = WeatherConstants.MaxForecastDays;

            public Task<WeatherViewModel?> GetForecastAsync(string place, string country)
            {
                Requests.Add($"{place}|{country}");

                if (Unavailable.Contains(place))
                {
                    return Task.FromResult<WeatherViewModel?>(null);
                }

                var days = Enumerable.Range(-1, ForecastLength + 1)
                    .Select(offset => new WeatherDayViewModel
                    {
                        Date = DateTime.Today.AddDays(offset),
                        MinTemperature = 5,
                        MaxTemperature = 15,
                        Description = "Clear sky",
                        Icon = "bi-sun"
                    })
                    .ToList();

                return Task.FromResult<WeatherViewModel?>(new WeatherViewModel
                {
                    Location = $"{place}, {country}",
                    Description = "Clear sky",
                    Icon = "bi-sun",
                    Days = days
                });
            }
        }
    }
}
