
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Voyage.Web.ViewModels.Packing
{
    public class PackingItemEditViewModel : PackingItemFormModel
    {
        [BindNever]
        [ValidateNever]
        public int Id { get; set; }

        [BindNever]
        [ValidateNever]
        public int TripId { get; set; }

        [BindNever]
        [ValidateNever]
        public string TripName { get; set; } = null!;
    }
}
