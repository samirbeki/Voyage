using System.ComponentModel.DataAnnotations;
using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Destinations
{
    public class DestinationFormViewModel
    {
        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ValidationConstants.DestinationNameMaxLength,
            MinimumLength = ValidationConstants.DestinationNameMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ValidationConstants.DestinationCountryMaxLength,
            MinimumLength = ValidationConstants.DestinationCountryMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Country { get; set; } = null!;

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ValidationConstants.DestinationDescriptionMaxLength,
            MinimumLength = ValidationConstants.DestinationDescriptionMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Description { get; set; } = null!;

        [RegularExpression(
            @"^(https?://.+|/images/.+)$",
            ErrorMessage = ErrorMessages.InvalidImageUrl)]
        [StringLength(
            ValidationConstants.ImageUrlMaxLength,
            ErrorMessage = ErrorMessages.MaxLengthField)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = ErrorMessages.CategoryRequired)]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IEnumerable<CategoryOptionViewModel> Categories { get; set; }
            = new List<CategoryOptionViewModel>();
    }
}
