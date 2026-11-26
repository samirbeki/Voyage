using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Voyage.Services.Contracts;
using Voyage.Services.Helpers;
using Voyage.Web.ViewModels.Weather;

namespace Voyage.Services
{
    public class WeatherService : IWeatherService
    {
        private const string GeocodingUrl = "https://geocoding-api.open-meteo.com/v1/search";
        private const string ForecastUrl = "https://api.open-meteo.com/v1/forecast";
        private const int ForecastDays = 5;

        private static readonly TimeSpan SuccessCacheDuration = TimeSpan.FromHours(1);
        private static readonly TimeSpan FailureCacheDuration = TimeSpan.FromMinutes(5);
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient httpClient;
        private readonly IMemoryCache cache;
        private readonly ILogger<WeatherService> logger;

        public WeatherService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<WeatherService> logger)
        {
            this.httpClient = httpClient;
            this.cache = cache;
            this.logger = logger;
        }

        public async Task<WeatherViewModel?> GetForecastAsync(string place, string country)
        {
            if (string.IsNullOrWhiteSpace(place))
            {
                return null;
            }

            string cacheKey = $"weather:{place.Trim()}:{country?.Trim()}".ToLowerInvariant();

            if (cache.TryGetValue(cacheKey, out WeatherViewModel? cached))
            {
                return cached;
            }

            WeatherViewModel? weather = null;

            try
            {
                weather = await LoadForecastAsync(place.Trim(), country?.Trim() ?? string.Empty);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                logger.LogWarning(ex, "Weather for {Place}, {Country} could not be loaded.", place, country);
            }

            cache.Set(cacheKey, weather, weather == null ? FailureCacheDuration : SuccessCacheDuration);

            return weather;
        }

        private async Task<WeatherViewModel?> LoadForecastAsync(string place, string country)
        {
            var location = await FindLocationAsync(place, country);

            int spaceIndex = place.IndexOf(' ');

            if (location == null && spaceIndex > 0)
            {
                location = await FindLocationAsync(place[..spaceIndex], country);
            }

            if (location == null)
            {
                return null;
            }

            string coordinates = FormattableString.Invariant(
                $"latitude={location.Latitude}&longitude={location.Longitude}");

            string forecastRequest = $"{ForecastUrl}?{coordinates}" +
                "&current=temperature_2m,weather_code,wind_speed_10m" +
                "&daily=weather_code,temperature_2m_max,temperature_2m_min" +
                $"&timezone=auto&forecast_days={ForecastDays}";

            var forecast = await httpClient.GetFromJsonAsync<ForecastResponse>(forecastRequest, JsonOptions);

            if (forecast?.Current == null || forecast.Daily == null)
            {
                return null;
            }

            var current = WeatherCodes.Describe(forecast.Current.WeatherCode);
            var daily = forecast.Daily;
            int daysCount = new[]
            {
                daily.Time.Count,
                daily.WeatherCode.Count,
                daily.TemperatureMax.Count,
                daily.TemperatureMin.Count
            }.Min();

            var days = new List<WeatherDayViewModel>();

            for (int i = 0; i < daysCount; i++)
            {
                var description = WeatherCodes.Describe(daily.WeatherCode[i]);

                days.Add(new WeatherDayViewModel
                {
                    Date = DateTime.Parse(daily.Time[i], CultureInfo.InvariantCulture),
                    MinTemperature = daily.TemperatureMin[i],
                    MaxTemperature = daily.TemperatureMax[i],
                    Description = description.Description,
                    Icon = description.Icon
                });
            }

            return new WeatherViewModel
            {
                Location = string.IsNullOrWhiteSpace(location.Country)
                    ? location.Name
                    : $"{location.Name}, {location.Country}",
                CurrentTemperature = forecast.Current.Temperature,
                WindSpeed = forecast.Current.WindSpeed,
                Description = current.Description,
                Icon = current.Icon,
                Days = days
            };
        }

        private async Task<GeocodingResult?> FindLocationAsync(string name, string country)
        {
            string request =
                $"{GeocodingUrl}?name={Uri.EscapeDataString(name)}&count=10&language=en&format=json";

            var geocoding = await httpClient.GetFromJsonAsync<GeocodingResponse>(request, JsonOptions);

            return geocoding?.Results?
                .OrderByDescending(r => string.Equals(r.Country, country, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        private sealed record GeocodingResponse(List<GeocodingResult>? Results);

        private sealed record GeocodingResult(
            string Name,
            double Latitude,
            double Longitude,
            string? Country);

        private sealed record ForecastResponse(CurrentWeather? Current, DailyWeather? Daily);

        private sealed record CurrentWeather(
            [property: JsonPropertyName("temperature_2m")] double Temperature,
            [property: JsonPropertyName("weather_code")] int WeatherCode,
            [property: JsonPropertyName("wind_speed_10m")] double WindSpeed);

        private sealed record DailyWeather(
            [property: JsonPropertyName("time")] List<string> Time,
            [property: JsonPropertyName("weather_code")] List<int> WeatherCode,
            [property: JsonPropertyName("temperature_2m_max")] List<double> TemperatureMax,
            [property: JsonPropertyName("temperature_2m_min")] List<double> TemperatureMin);
    }
}
