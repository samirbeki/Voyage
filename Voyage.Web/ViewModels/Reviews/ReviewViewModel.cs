namespace Voyage.Web.ViewModels.Reviews
{
    public class ReviewViewModel
    {
        public int Id { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = null!;

        public string AuthorName { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public bool IsEdited { get; set; }

        public bool IsMine { get; set; }

        public bool IsHidden { get; set; }
    }
}