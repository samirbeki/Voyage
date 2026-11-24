using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Web.ViewModels.Reviews;
using Xunit;

namespace Voyage.Services.Tests
{
    public class ReviewFormModelTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void Validation_RatingInsideTheRange_HasNoErrors(int rating)
        {
            Assert.Empty(Validate(new ReviewFormModel { Rating = rating, Comment = "Great views and friendly people." }));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void Validation_RatingOutsideTheRange_ReturnsError(int rating)
        {
            var error = Assert.Single(Validate(new ReviewFormModel { Rating = rating, Comment = "Great views and friendly people." }));

            var expected = string.Format(
                ErrorMessages.ReviewRatingRange,
                nameof(ReviewFormModel.Rating),
                ReviewValidationConstants.RatingMin,
                ReviewValidationConstants.RatingMax);

            Assert.Equal(expected, error.ErrorMessage);
        }

        [Fact]
        public void Validation_MissingRating_ReturnsError()
        {
            var error = Assert.Single(Validate(new ReviewFormModel { Comment = "Great views and friendly people." }));

            Assert.Equal(ErrorMessages.ReviewRatingRequired, error.ErrorMessage);
        }

        [Fact]
        public void Validation_TooShortComment_ReturnsError()
        {
            var error = Assert.Single(Validate(new ReviewFormModel { Rating = 4, Comment = "Nice." }));

            var expected = string.Format(
                ErrorMessages.ReviewCommentLength,
                nameof(ReviewFormModel.Comment),
                ReviewValidationConstants.CommentMaxLength,
                ReviewValidationConstants.CommentMinLength);

            Assert.Equal(expected, error.ErrorMessage);
        }

        private static List<ValidationResult> Validate(ReviewFormModel model)
        {
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            return results;
        }
    }
}
