using System.ComponentModel.DataAnnotations;
using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Destinations
{
    public class DestinationFormViewModel
    {
        [Required]
        [StringLength(
            ValidationConstants.DestinationNameMaxLength,
            MinimumLength = ValidationConstants.DestinationNameMinLength)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(
            ValidationConstants.DestinationCountryMaxLength,
            MinimumLength = ValidationConstants.DestinationCountryMinLength)]
        public string Country { get; set; } = null!;

        [Required]
        [StringLength(
            ValidationConstants.DestinationDescriptionMaxLength,
            MinimumLength = ValidationConstants.DestinationDescriptionMinLength)]
        public string Description { get; set; } = null!;

        [RegularExpression(
            @"^(https?://.+|/images/.+)$",
            ErrorMessage = "Enter a full link (https://...) or a path starting with /images.")]
        [StringLength(ValidationConstants.ImageUrlMaxLength)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IEnumerable<CategoryOptionViewModel> Categories { get; set; }
            = new List<CategoryOptionViewModel>();
    }
}
