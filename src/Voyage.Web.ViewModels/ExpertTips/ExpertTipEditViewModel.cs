using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Voyage.Web.ViewModels.ExpertTips
{
    public class ExpertTipEditViewModel : ExpertTipFormModel
    {
        [BindNever]
        [ValidateNever]
        public int Id { get; set; }

        [BindNever]
        [ValidateNever]
        public int DestinationId { get; set; }

        [BindNever]
        [ValidateNever]
        public string DestinationName { get; set; } = null!;
    }
}
