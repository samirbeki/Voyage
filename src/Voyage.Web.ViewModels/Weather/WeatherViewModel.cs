namespace Voyage.Web.ViewModels.Weather
{
    public class WeatherViewModel
    {
        private const double MinRangeWidth = 8;

        public string Location { get; set; } = null!;

        public double CurrentTemperature { get; set; }

        public double WindSpeed { get; set; }

        public string Description { get; set; } = null!;

        public string Icon { get; set; } = null!;

        public IEnumerable<WeatherDayViewModel> Days { get; set; }
            = new List<WeatherDayViewModel>();

        public double ForecastMin => Days.Any() ? Days.Min(d => d.MinTemperature) : 0;

        public double ForecastMax => Days.Any() ? Days.Max(d => d.MaxTemperature) : 0;

        public double RangeStart(WeatherDayViewModel day)
            => Math.Min(ToPercent(day.MinTemperature - ForecastMin), 100 - MinRangeWidth);

        public double RangeWidth(WeatherDayViewModel day)
        {
            var width = Math.Max(ToPercent(day.MaxTemperature - day.MinTemperature), MinRangeWidth);

            return Math.Min(width, 100 - RangeStart(day));
        }

        public WeatherViewModel FirstDays(int count)
            => WithDays(Days.Take(count));

        public WeatherViewModel ForPeriod(DateTime from, DateTime to)
            => WithDays(Days.Where(d => d.Date.Date >= from.Date && d.Date.Date <= to.Date));

        private WeatherViewModel WithDays(IEnumerable<WeatherDayViewModel> days)
            => new()
            {
                Location = Location,
                CurrentTemperature = CurrentTemperature,
                WindSpeed = WindSpeed,
                Description = Description,
                Icon = Icon,
                Days = days.ToList()
            };

        private double ToPercent(double value)
            => Math.Round(value / Math.Max(ForecastMax - ForecastMin, 1) * 100, 1);
    }
}
