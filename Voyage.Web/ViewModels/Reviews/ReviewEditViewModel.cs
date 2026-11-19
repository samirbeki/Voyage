namespace Voyage.Web.ViewModels.Reviews
{
    public class ReviewEditViewModel : ReviewFormModel
    {
        public int Id { get; set; }

        public int DestinationId { get; set; }

        public string DestinationName { get; set; } = null!;
    }
}