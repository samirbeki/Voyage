using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;

namespace Voyage.Web.ViewModels.Destinations
{
    public class DestinationFormViewModel : IValidatableObject
    {
        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            DestinationValidationConstants.NameMaxLength,
            MinimumLength = DestinationValidationConstants.NameMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            DestinationValidationConstants.CountryMaxLength,
            MinimumLength = DestinationValidationConstants.CountryMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Country { get; set; } = null!;

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            DestinationValidationConstants.DescriptionMaxLength,
            MinimumLength = DestinationValidationConstants.DescriptionMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Description { get; set; } = null!;

        [RegularExpression(
            @"^(https?://.+|/images/.+)$",
            ErrorMessage = ErrorMessages.InvalidImageUrl)]
        [StringLength(
            SharedValidationConstants.ImageUrlMaxLength,
            ErrorMessage = ErrorMessages.MaxLengthField)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Range(
            DestinationValidationConstants.LatitudeMin,
            DestinationValidationConstants.LatitudeMax,
            ErrorMessage = ErrorMessages.CoordinateRange)]
        public double? Latitude { get; set; }

        [Range(
            DestinationValidationConstants.LongitudeMin,
            DestinationValidationConstants.LongitudeMax,
            ErrorMessage = ErrorMessages.CoordinateRange)]
        public double? Longitude { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = ErrorMessages.CategoryRequired)]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IEnumerable<CategoryOptionViewModel> Categories { get; set; }
            = new List<CategoryOptionViewModel>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Latitude.HasValue != Longitude.HasValue)
            {
                yield return new ValidationResult(
                    ErrorMessages.CoordinatesIncomplete,
                    new[] { Latitude.HasValue ? nameof(Longitude) : nameof(Latitude) });
            }
        }
    }
}
