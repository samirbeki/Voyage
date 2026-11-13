namespace Voyage.Web.ViewModels.Categories
{
    public class CategoryAllViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int DestinationsCount { get; set; }
    }
}