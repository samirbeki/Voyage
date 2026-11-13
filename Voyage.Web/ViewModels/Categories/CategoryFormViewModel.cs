using System.ComponentModel.DataAnnotations;
using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Categories
{
    public class CategoryFormViewModel
    {
        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ValidationConstants.CategoryNameMaxLength,
            MinimumLength = ValidationConstants.CategoryNameMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Name { get; set; } = null!;

        [StringLength(
            ValidationConstants.CategoryDescriptionMaxLength,
            ErrorMessage = ErrorMessages.MaxLengthField)]
        public string? Description { get; set; }
    }
}