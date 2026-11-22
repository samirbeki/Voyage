using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;

namespace Voyage.Web.ViewModels.Trips
{
    public class TripFormViewModel : IValidatableObject
    {
        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            TripValidationConstants.TitleMaxLength,
            MinimumLength = TripValidationConstants.TitleMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Title { get; set; } = null!;

        [StringLength(
            TripValidationConstants.DescriptionMaxLength,
            ErrorMessage = ErrorMessages.MaxLengthField)]
        public string? Description { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [Range(
            typeof(decimal),
            TripValidationConstants.BudgetMinValue,
            TripValidationConstants.BudgetMaxValue,
            ErrorMessage = ErrorMessages.BudgetRange)]
        public decimal? Budget { get; set; }

        [RegularExpression(
            @"^(https?://.+|/images/.+)$",
            ErrorMessage = ErrorMessages.InvalidImageUrl)]
        [StringLength(
            SharedValidationConstants.ImageUrlMaxLength,
            ErrorMessage = ErrorMessages.MaxLengthField)]
        [Display(Name = "Cover image URL")]
        public string? CoverImageUrl { get; set; }

        [Display(Name = "Destinations")]
        public List<int> SelectedDestinationIds { get; set; }
            = new List<int>();

        public IEnumerable<DestinationOptionViewModel> AvailableDestinations { get; set; }
            = new List<DestinationOptionViewModel>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    ErrorMessages.EndDateBeforeStartDate,
                    new[] { nameof(EndDate) });
            }

            if ((EndDate - StartDate).TotalDays > TripValidationConstants.MaxDays)
            {
                yield return new ValidationResult(
                    ErrorMessages.TripTooLong,
                    new[] { nameof(EndDate) });
            }

            if (!SelectedDestinationIds.Any())
            {
                yield return new ValidationResult(
                    ErrorMessages.DestinationsRequired,
                    new[] { nameof(SelectedDestinationIds) });
            }
        }
    }
}