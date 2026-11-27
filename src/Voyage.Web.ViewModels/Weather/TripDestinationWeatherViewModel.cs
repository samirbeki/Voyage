namespace Voyage.Web.ViewModels.Weather
{
    public class TripDestinationWeatherViewModel
    {
        public int DestinationId { get; set; }

        public string Name { get; set; } = null!;

        public string Country { get; set; } = null!;

        public WeatherViewModel? Weather { get; set; }
    }
}
