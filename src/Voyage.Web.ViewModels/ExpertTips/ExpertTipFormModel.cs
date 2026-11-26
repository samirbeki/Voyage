using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;

namespace Voyage.Web.ViewModels.ExpertTips
{
    public class ExpertTipFormModel
    {
        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ExpertTipValidationConstants.ContentMaxLength,
            MinimumLength = ExpertTipValidationConstants.ContentMinLength,
            ErrorMessage = ErrorMessages.ExpertTipContentLength)]
        [Display(Name = "Your tip")]
        public string Content { get; set; } = null!;
    }
}
