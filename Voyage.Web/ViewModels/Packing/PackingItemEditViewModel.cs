namespace Voyage.Web.ViewModels.Packing
{
    public class PackingItemEditViewModel : PackingItemFormModel
    {
        public int Id { get; set; }

        public int TripId { get; set; }

        public string TripName { get; set; } = null!;
    }
}