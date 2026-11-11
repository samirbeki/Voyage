namespace Voyage.Web.ViewModels.Trips
{
    public class TripDestinationViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Country { get; set; } = null!;

        public string? ImageUrl { get; set; }
    }
}