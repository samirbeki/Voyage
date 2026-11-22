namespace Voyage.Web.ViewModels.Reviews
{
    public class ModerationReviewsPageViewModel
    {
        public IEnumerable<ModerationReviewViewModel> Reviews { get; set; }
            = new List<ModerationReviewViewModel>();

        public string Status { get; set; } = "all";

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }
    }
}