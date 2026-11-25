namespace Voyage.Web.ViewModels.Statistics
{
    public class CountryStatisticViewModel
    {
        public string Country { get; set; } = null!;

        public int TripsCount { get; set; }

        public int Percent { get; set; }
    }
}
