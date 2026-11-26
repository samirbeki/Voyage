namespace Voyage.Web.ViewModels.ExpertTips
{
    public class ExpertTipViewModel
    {
        public int Id { get; set; }

        public string Content { get; set; } = null!;

        public string AuthorName { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public bool IsEdited { get; set; }

        public bool IsMine { get; set; }
    }
}
