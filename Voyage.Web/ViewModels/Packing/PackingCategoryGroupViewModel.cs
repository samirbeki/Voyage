using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Packing
{
    public class PackingCategoryGroupViewModel
    {
        public PackingCategory Category { get; set; }

        public IEnumerable<PackingItemViewModel> Items { get; set; }
            = new List<PackingItemViewModel>();

        public int PackedCount { get; set; }

        public int TotalCount { get; set; }
    }
}