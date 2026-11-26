namespace Voyage.Web.ViewModels.Weather
{
    public class WeatherDayViewModel
    {
        public DateTime Date { get; set; }

        public double MinTemperature { get; set; }

        public double MaxTemperature { get; set; }

        public string Description { get; set; } = null!;

        public string Icon { get; set; } = null!;

        public string Tone => Icon switch
        {
            var icon when icon.Contains("lightning") => "stormy",
            var icon when icon.Contains("snow") => "snowy",
            var icon when icon.Contains("rain") || icon.Contains("drizzle") || icon.Contains("sleet") => "rainy",
            var icon when icon.Contains("fog") => "foggy",
            var icon when icon.Contains("sun") => "sunny",
            _ => "cloudy"
        };
    }
}
