namespace Voyage.Web.ViewModels.TravelExperts
{
    public class TravelExpertsPageViewModel
    {
        public string? Search { get; set; }

        public int ExpertsCount { get; set; }

        public IEnumerable<TravelExpertUserViewModel> Users { get; set; }
            = new List<TravelExpertUserViewModel>();
    }
}
