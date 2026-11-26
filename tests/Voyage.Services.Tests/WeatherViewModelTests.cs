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

        private static WeatherViewModel CreateModel(params WeatherDayViewModel[] days)
            => new() { Days = days };

        private static WeatherDayViewModel Day(double min, double max)
            => new() { MinTemperature = min, MaxTemperature = max, Icon = "bi-sun", Description = "Clear sky" };
    }
}
