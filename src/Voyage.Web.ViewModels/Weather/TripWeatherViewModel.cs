namespace Voyage.Web.ViewModels.Weather
{
    public class TripWeatherViewModel
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime AvailableFrom { get; set; }

        public DateTime CoveredUntil { get; set; }

        public bool IsFinished { get; set; }

        public bool IsTooEarly { get; set; }

        public bool IsPartial => !IsFinished && !IsTooEarly && CoveredUntil.Date < EndDate.Date;

        public IEnumerable<TripDestinationWeatherViewModel> Destinations { get; set; }
            = new List<TripDestinationWeatherViewModel>();
    }
}
