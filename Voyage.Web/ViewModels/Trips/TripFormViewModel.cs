using System.ComponentModel.DataAnnotations;
using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Trips
{
    public class TripFormViewModel : IValidatableObject
    {
        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ValidationConstants.TripTitleMaxLength,
            MinimumLength = ValidationConstants.TripTitleMinLength,
            ErrorMessage = ErrorMessages.StringLengthRange)]
        public string Title { get; set; } = null!;

        [StringLength(
            ValidationConstants.TripDescriptionMaxLength,
            ErrorMessage = ErrorMessages.MaxLengthField)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        [Range(
            typeof(decimal),
            ValidationConstants.TripBudgetMinValue,
            ValidationConstants.TripBudgetMaxValue,
            ErrorMessage = ErrorMessages.BudgetRange)]
        public decimal Budget { get; set; }

        [RegularExpression(
            @"^(https?://.+|/images/.+)$",
            ErrorMessage = ErrorMessages.InvalidImageUrl)]
        [StringLength(
            ValidationConstants.ImageUrlMaxLength,
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

            if ((EndDate - StartDate).TotalDays > ValidationConstants.TripMaxDays)
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