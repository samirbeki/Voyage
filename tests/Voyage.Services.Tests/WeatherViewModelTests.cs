using Voyage.Web.ViewModels.Weather;
using Xunit;

namespace Voyage.Services.Tests
{
    public class WeatherViewModelTests
    {
        [Theory]
        [InlineData("bi-sun", "sunny")]
        [InlineData("bi-cloud-sun", "sunny")]
        [InlineData("bi-clouds", "cloudy")]
        [InlineData("bi-cloud", "cloudy")]
        [InlineData("bi-cloud-fog", "foggy")]
        [InlineData("bi-cloud-drizzle", "rainy")]
        [InlineData("bi-cloud-rain-heavy", "rainy")]
        [InlineData("bi-cloud-sleet", "rainy")]
        [InlineData("bi-cloud-snow", "snowy")]
        [InlineData("bi-cloud-lightning-rain", "stormy")]
        public void Tone_MatchesWeatherIcon(string icon, string expectedTone)
        {
            var day = new WeatherDayViewModel { Icon = icon };

            Assert.Equal(expectedTone, day.Tone);
        }

        [Fact]
        public void ForecastMinAndMax_UseAllDays()
        {
            var model = CreateModel(Day(4, 13), Day(11, 17), Day(8, 13));

            Assert.Equal(4, model.ForecastMin);
            Assert.Equal(17, model.ForecastMax);
        }

        [Fact]
        public void ForecastMinAndMax_NoDays_ReturnZero()
        {
            var model = new WeatherViewModel();

            Assert.Equal(0, model.ForecastMin);
            Assert.Equal(0, model.ForecastMax);
        }

        [Fact]
        public void Range_ColdestDay_StartsAtZero()
        {
            var coldest = Day(10, 20);
            var model = CreateModel(coldest, Day(15, 30));

            Assert.Equal(0, model.RangeStart(coldest));
            Assert.Equal(50, model.RangeWidth(coldest));
        }

        [Fact]
        public void Range_WarmestDay_EndsAtHundred()
        {
            var warmest = Day(15, 30);
            var model = CreateModel(Day(10, 20), warmest);

            Assert.Equal(25, model.RangeStart(warmest));
            Assert.Equal(75, model.RangeWidth(warmest));
        }

        [Fact]
        public void RangeWidth_SmallDifference_UsesMinimumWidth()
        {
            var steady = Day(20, 20.2);
            var model = CreateModel(Day(10, 30), steady);

            Assert.Equal(8, model.RangeWidth(steady));
        }

        [Fact]
        public void Range_NeverGoesPastTheEnd()
        {
            var warmest = Day(29.9, 30);
            var model = CreateModel(Day(10, 20), warmest);

            Assert.True(model.RangeStart(warmest) + model.RangeWidth(warmest) <= 100);
        }

        [Fact]
        public void Range_SameTemperatureAllWeek_DoesNotDivideByZero()
        {
            var day = Day(15, 15);
            var model = CreateModel(day);

            Assert.Equal(0, model.RangeStart(day));
            Assert.Equal(8, model.RangeWidth(day));
        }

        [Fact]
        public void FirstDays_KeepsOnlyTheFirstDaysAndTheCurrentWeather()
        {
            var model = Forecast(7);

            var result = model.FirstDays(5);

            Assert.Equal(5, result.Days.Count());
            Assert.Equal(model.Days.First().Date, result.Days.First().Date);
            Assert.Equal("Lisbon, Portugal", result.Location);
            Assert.Equal(18.4, result.CurrentTemperature);
            Assert.Equal(12.1, result.WindSpeed);
            Assert.Equal("Partly cloudy", result.Description);
            Assert.Equal("bi-cloud-sun", result.Icon);
        }

        [Fact]
        public void FirstDays_MoreThanAvailable_ReturnsAllDays()
        {
            var result = Forecast(3).FirstDays(5);

            Assert.Equal(3, result.Days.Count());
        }

        [Fact]
        public void FirstDays_DoesNotChangeTheOriginal()
        {
            var model = Forecast(7);

            model.FirstDays(2);

            Assert.Equal(7, model.Days.Count());
        }

        [Fact]
        public void ForPeriod_KeepsTheDaysInTheRangeIncludingBothEnds()
        {
            var model = Forecast(10);
            var start = new DateTime(2026, 12, 3);

            var result = model.ForPeriod(start, start.AddDays(2));

            Assert.Equal(
                new[] { start, start.AddDays(1), start.AddDays(2) },
                result.Days.Select(d => d.Date));
        }

        [Fact]
        public void ForPeriod_IgnoresTheTimeOfDay()
        {
            var model = Forecast(10);

            var result = model.ForPeriod(new DateTime(2026, 12, 3, 18, 30, 0), new DateTime(2026, 12, 4, 6, 0, 0));

            Assert.Equal(2, result.Days.Count());
        }

        [Fact]
        public void ForPeriod_NoMatchingDays_ReturnsNoDays()
        {
            var result = Forecast(5).ForPeriod(new DateTime(2027, 1, 1), new DateTime(2027, 1, 5));

            Assert.Empty(result.Days);
        }

        [Fact]
        public void ForPeriod_RangeUsesOnlyTheSelectedDays()
        {
            var model = CreateModel(
                DayOn(new DateTime(2026, 12, 1), 0, 30),
                DayOn(new DateTime(2026, 12, 2), 10, 20),
                DayOn(new DateTime(2026, 12, 3), 12, 18));

            var result = model.ForPeriod(new DateTime(2026, 12, 2), new DateTime(2026, 12, 3));

            Assert.Equal(10, result.ForecastMin);
            Assert.Equal(20, result.ForecastMax);
        }

        private static WeatherViewModel Forecast(int days)
            => new()
            {
                Location = "Lisbon, Portugal",
                CurrentTemperature = 18.4,
                WindSpeed = 12.1,
                Description = "Partly cloudy",
                Icon = "bi-cloud-sun",
                Days = Enumerable.Range(0, days)
                    .Select(i => DayOn(new DateTime(2026, 12, 1).AddDays(i), 10, 18))
                    .ToList()
            };

        private static WeatherDayViewModel DayOn(DateTime date, double min, double max)
        {
            var day = Day(min, max);
            day.Date = date;

            return day;
        }

        private static WeatherViewModel CreateModel(params WeatherDayViewModel[] days)
            => new() { Days = days };

        private static WeatherDayViewModel Day(double min, double max)
            => new() { MinTemperature = min, MaxTemperature = max, Icon = "bi-sun", Description = "Clear sky" };
    }
}
