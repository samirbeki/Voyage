using System.ComponentModel.DataAnnotations;
using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Reviews
{
    public class ReviewFormModel
    {
        [Required(ErrorMessage = ErrorMessages.ReviewRatingRequired)]
        [Range(
            ValidationConstants.ReviewRatingMin,
            ValidationConstants.ReviewRatingMax,
            ErrorMessage = ErrorMessages.ReviewRatingRange)]
        public int? Rating { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ValidationConstants.ReviewCommentMaxLength,
            MinimumLength = ValidationConstants.ReviewCommentMinLength,
            ErrorMessage = ErrorMessages.ReviewCommentLength)]
        public string Comment { get; set; } = null!;
    }
}