using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Voyage.Services.Tests
{
    public class WeatherServiceTests : IDisposable
    {
        private const string RomeGeocoding = """
            {"results":[{"name":"Rome","latitude":41.89193,"longitude":12.51133,"country":"Italy"}]}
            """;

        private const string ParisGeocoding = """
            {"results":[
                {"name":"Paris","latitude":33.66094,"longitude":-95.55551,"country":"United States"},
                {"name":"Paris","latitude":48.85341,"longitude":2.3488,"country":"France"}
            ]}
            """;

        private const string Forecast = """
            {
              "current":{"time":"2026-11-26T13:45","temperature_2m":14.6,"weather_code":2,"wind_speed_10m":11.4},
              "daily":{
                "time":["2026-11-26","2026-11-27","2026-11-28","2026-11-29","2026-11-30"],
                "weather_code":[2,61,0,3,95],
                "temperature_2m_max":[16.2,13.8,15.1,14.4,12.9],
                "temperature_2m_min":[8.4,9.1,6.7,7.3,8.8]
              }
            }
            """;

        private readonly FakeHandler handler = new();
        private readonly MemoryCache cache = new(new MemoryCacheOptions());
        private readonly WeatherService service;

        public WeatherServiceTests()
        {
            service = new WeatherService(new HttpClient(handler), cache, NullLogger<WeatherService>.Instance);
        }

        public void Dispose()
        {
            cache.Dispose();
            handler.Dispose();
        }

        [Fact]
        public async Task GetForecastAsync_ReturnsCurrentWeatherAndFiveDays()
        {
            handler.Respond("geocoding-api", RomeGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);

            var result = await service.GetForecastAsync("Rome", "Italy");

            Assert.NotNull(result);
            Assert.Equal("Rome, Italy", result.Location);
            Assert.Equal(14.6, result.CurrentTemperature);
            Assert.Equal(11.4, result.WindSpeed);
            Assert.Equal("Partly cloudy", result.Description);
            Assert.Equal("bi-cloud-sun", result.Icon);

            var days = result.Days.ToList();
            Assert.Equal(5, days.Count);
            Assert.Equal(new DateTime(2026, 11, 26), days[0].Date);
            Assert.Equal(16.2, days[0].MaxTemperature);
            Assert.Equal(8.4, days[0].MinTemperature);
            Assert.Equal(
                new[] { "Partly cloudy", "Rain", "Clear sky", "Overcast", "Thunderstorm" },
                days.Select(d => d.Description));
        }

        [Fact]
        public async Task GetForecastAsync_AsksForTheLongestForecast()
        {
            handler.Respond("geocoding-api", RomeGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);

            await service.GetForecastAsync("Rome", "Italy");

            Assert.Contains("forecast_days=16", handler.RequestedUrls.Last());
        }

        [Fact]
        public async Task GetForecastAsync_SeveralPlacesWithTheSameName_PicksTheOneInTheGivenCountry()
        {
            handler.Respond("geocoding-api", ParisGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);

            var result = await service.GetForecastAsync("Paris", "France");

            Assert.Equal("Paris, France", result!.Location);
            Assert.Contains("latitude=48.85341", handler.RequestedUrls.Last());
            Assert.Contains("longitude=2.3488", handler.RequestedUrls.Last());
        }

        [Fact]
        public async Task GetForecastAsync_CountryNotFound_UsesTheFirstResult()
        {
            handler.Respond("geocoding-api", ParisGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);

            var result = await service.GetForecastAsync("Paris", "Narnia");

            Assert.Equal("Paris, United States", result!.Location);
        }

        [Fact]
        public async Task GetForecastAsync_SendsCoordinatesWithDotEvenWithBulgarianCulture()
        {
            handler.Respond("geocoding-api", RomeGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);
            var originalCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("bg-BG");

                await service.GetForecastAsync("Rome", "Italy");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }

            Assert.Contains("latitude=41.89193&longitude=12.51133", handler.RequestedUrls.Last());
        }

        [Fact]
        public async Task GetForecastAsync_EncodesThePlaceName()
        {
            handler.Respond("geocoding-api", RomeGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);

            await service.GetForecastAsync("Plitvice Lakes", "Croatia");

            Assert.Contains("name=Plitvice%20Lakes", handler.RequestedUrls.First());
        }

        [Fact]
        public async Task GetForecastAsync_UnknownPlace_ReturnsNullWithoutAskingForForecast()
        {
            handler.Respond("geocoding-api", """{"generationtime_ms":0.5}""");

            var result = await service.GetForecastAsync("Atlantis", "Nowhere");

            Assert.Null(result);
            Assert.Single(handler.RequestedUrls);
        }

        [Fact]
        public async Task GetForecastAsync_FullNameNotFound_TriesTheFirstWord()
        {
            handler.Respond("name=Rila%20Lakes", """{"generationtime_ms":0.4}""");
            handler.Respond("name=Rila&", """
                {"results":[{"name":"Rila","latitude":42.13333,"longitude":23.13333,"country":"Bulgaria"}]}
                """);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);

            var result = await service.GetForecastAsync("Rila Lakes", "Bulgaria");

            Assert.Equal("Rila, Bulgaria", result!.Location);
            Assert.Equal(3, handler.RequestedUrls.Count);
        }

        [Fact]
        public async Task GetForecastAsync_NeitherFullNameNorFirstWordFound_ReturnsNull()
        {
            handler.Respond("geocoding-api", """{"generationtime_ms":0.4}""");

            var result = await service.GetForecastAsync("Lost Valley", "Nowhere");

            Assert.Null(result);
            Assert.Equal(2, handler.RequestedUrls.Count);
            Assert.Contains("name=Lost&", handler.RequestedUrls[1]);
        }

        [Fact]
        public async Task GetForecastAsync_FullNameFound_DoesNotSearchAgain()
        {
            handler.Respond("geocoding-api", RomeGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);

            await service.GetForecastAsync("Plitvice Lakes", "Croatia");

            Assert.Equal(2, handler.RequestedUrls.Count);
        }

        [Fact]
        public async Task GetForecastAsync_GeocodingFails_ReturnsNull()
        {
            handler.Respond("geocoding-api", "error", HttpStatusCode.InternalServerError);

            Assert.Null(await service.GetForecastAsync("Rome", "Italy"));
        }

        [Fact]
        public async Task GetForecastAsync_ForecastFails_ReturnsNull()
        {
            handler.Respond("geocoding-api", RomeGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", "error", HttpStatusCode.ServiceUnavailable);

            Assert.Null(await service.GetForecastAsync("Rome", "Italy"));
        }

        [Fact]
        public async Task GetForecastAsync_InvalidJson_ReturnsNull()
        {
            handler.Respond("geocoding-api", RomeGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", "{ this is not json");

            Assert.Null(await service.GetForecastAsync("Rome", "Italy"));
        }

        [Fact]
        public async Task GetForecastAsync_NoConnection_ReturnsNull()
        {
            handler.ThrowOnRequest = true;

            Assert.Null(await service.GetForecastAsync("Rome", "Italy"));
        }

        [Fact]
        public async Task GetForecastAsync_SecondCall_IsServedFromCache()
        {
            handler.Respond("geocoding-api", RomeGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);

            var first = await service.GetForecastAsync("Rome", "Italy");
            var second = await service.GetForecastAsync("  ROME ", "italy");

            Assert.Equal(2, handler.RequestedUrls.Count);
            Assert.Same(first, second);
        }

        [Fact]
        public async Task GetForecastAsync_FailedCall_IsAlsoCachedForAShortTime()
        {
            handler.Respond("geocoding-api", "error", HttpStatusCode.InternalServerError);

            await service.GetForecastAsync("Rome", "Italy");
            await service.GetForecastAsync("Rome", "Italy");

            Assert.Single(handler.RequestedUrls);
        }

        [Fact]
        public async Task GetForecastAsync_DifferentPlaces_AreCachedSeparately()
        {
            handler.Respond("geocoding-api", RomeGeocoding);
            handler.Respond("api.open-meteo.com/v1/forecast", Forecast);

            await service.GetForecastAsync("Rome", "Italy");
            await service.GetForecastAsync("Florence", "Italy");

            Assert.Equal(4, handler.RequestedUrls.Count);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetForecastAsync_EmptyPlace_ReturnsNullWithoutRequests(string? place)
        {
            Assert.Null(await service.GetForecastAsync(place!, "Italy"));
            Assert.Empty(handler.RequestedUrls);
        }

        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Dictionary<string, (string Body, HttpStatusCode Status)> responses = new();

            public List<string> RequestedUrls { get; } = new();

            public bool ThrowOnRequest { get; set; }

            public void Respond(string urlPart, string body, HttpStatusCode status = HttpStatusCode.OK)
            {
                responses[urlPart] = (body, status);
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                string url = request.RequestUri!.AbsoluteUri;
                RequestedUrls.Add(url);

                if (ThrowOnRequest)
                {
                    throw new HttpRequestException("No connection");
                }

                var match = responses.FirstOrDefault(r => url.Contains(r.Key));

                var response = match.Key == null
                    ? new HttpResponseMessage(HttpStatusCode.NotFound)
                    : new HttpResponseMessage(match.Value.Status)
                    {
                        Content = new StringContent(match.Value.Body, Encoding.UTF8, "application/json")
                    };

                return Task.FromResult(response);
            }
        }
    }
}
