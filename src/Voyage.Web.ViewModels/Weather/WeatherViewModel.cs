namespace Voyage.Web.ViewModels.Weather
{
    public class WeatherViewModel
    {
        public string Location { get; set; } = null!;

        public double CurrentTemperature { get; set; }

        public double WindSpeed { get; set; }

        public string Description { get; set; } = null!;

        public string Icon { get; set; } = null!;

        public IEnumerable<WeatherDayViewModel> Days { get; set; }
            = new List<WeatherDayViewModel>();
    }
}
