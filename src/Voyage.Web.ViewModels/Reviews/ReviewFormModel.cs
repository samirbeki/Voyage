using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;

namespace Voyage.Web.ViewModels.Reviews
{
    public class ReviewFormModel
    {
        [Required(ErrorMessage = ErrorMessages.ReviewRatingRequired)]
        [Range(
            ReviewValidationConstants.RatingMin,
            ReviewValidationConstants.RatingMax,
            ErrorMessage = ErrorMessages.ReviewRatingRange)]
        public int? Rating { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ReviewValidationConstants.CommentMaxLength,
            MinimumLength = ReviewValidationConstants.CommentMinLength,
            ErrorMessage = ErrorMessages.ReviewCommentLength)]
        public string Comment { get; set; } = null!;
    }
}
