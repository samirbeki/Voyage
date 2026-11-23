using Voyage.Data.Models.Enums;

namespace Voyage.Web.ViewModels.Packing
{
    public class PackingItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int Quantity { get; set; }

        public PackingCategory Category { get; set; }

        public bool IsPacked { get; set; }
    }
}
