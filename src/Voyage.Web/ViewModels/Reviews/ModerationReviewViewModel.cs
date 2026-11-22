namespace Voyage.Web.ViewModels.Reviews
{
    public class ModerationReviewViewModel
    {
        public int Id { get; set; }

        public int DestinationId { get; set; }

        public string DestinationName { get; set; } = null!;

        public string AuthorName { get; set; } = null!;

        public int Rating { get; set; }

        public string Comment { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public bool IsHidden { get; set; }
    }
}