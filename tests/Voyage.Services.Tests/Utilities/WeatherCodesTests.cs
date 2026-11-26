using Voyage.Services.Helpers;
using Xunit;

namespace Voyage.Services.Tests.Utilities
{
    public class WeatherCodesTests
    {
        [Theory]
        [InlineData(0, "Clear sky", "bi-sun")]
        [InlineData(1, "Mainly clear", "bi-sun")]
        [InlineData(2, "Partly cloudy", "bi-cloud-sun")]
        [InlineData(3, "Overcast", "bi-clouds")]
        [InlineData(45, "Fog", "bi-cloud-fog")]
        [InlineData(53, "Drizzle", "bi-cloud-drizzle")]
        [InlineData(57, "Freezing drizzle", "bi-cloud-sleet")]
        [InlineData(61, "Rain", "bi-cloud-rain")]
        [InlineData(65, "Heavy rain", "bi-cloud-rain-heavy")]
        [InlineData(73, "Snow", "bi-cloud-snow")]
        [InlineData(81, "Rain showers", "bi-cloud-rain-heavy")]
        [InlineData(86, "Snow showers", "bi-cloud-snow")]
        [InlineData(95, "Thunderstorm", "bi-cloud-lightning-rain")]
        [InlineData(99, "Thunderstorm with hail", "bi-cloud-lightning-rain")]
        public void Describe_KnownCode_ReturnsDescriptionAndIcon(int code, string description, string icon)
        {
            var result = WeatherCodes.Describe(code);

            Assert.Equal(description, result.Description);
            Assert.Equal(icon, result.Icon);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(100)]
        public void Describe_UnknownCode_ReturnsUnknown(int code)
        {
            var result = WeatherCodes.Describe(code);

            Assert.Equal("Unknown", result.Description);
            Assert.Equal("bi-cloud", result.Icon);
        }
    }
}
