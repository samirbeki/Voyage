namespace Voyage.Web.ViewModels.TravelExperts
{
    public class TravelExpertUserViewModel
    {
        public string Id { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int ReviewsCount { get; set; }

        public bool IsExpert { get; set; }
    }
}
