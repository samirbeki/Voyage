using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;

namespace Voyage.Web.ViewModels.Categories
{
    public class CategoryFormViewModel
    {
        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            CategoryValidationConstants.NameMaxLength,
            MinimumLength = CategoryValidationConstants.NameMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Name { get; set; } = null!;

        [StringLength(
            CategoryValidationConstants.DescriptionMaxLength,
            ErrorMessage = ErrorMessages.MaxLengthField)]
        public string? Description { get; set; }
    }
}
