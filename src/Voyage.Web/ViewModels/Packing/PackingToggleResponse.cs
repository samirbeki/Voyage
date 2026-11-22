namespace Voyage.Web.ViewModels.Packing
{
    public class PackingToggleResponse
    {
        public int ItemId { get; set; }

        public bool IsPacked { get; set; }

        public int Packed { get; set; }

        public int Total { get; set; }

        public int Percent { get; set; }

        public bool IsComplete { get; set; }
    }
}