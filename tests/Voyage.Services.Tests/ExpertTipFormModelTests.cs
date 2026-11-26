using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Web.ViewModels.ExpertTips;
using Xunit;

namespace Voyage.Services.Tests
{
    public class ExpertTipFormModelTests
    {
        [Fact]
        public void Validation_CorrectTip_HasNoErrors()
        {
            Assert.Empty(Validate("Book the Colosseum for the first slot of the day."));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validation_MissingContent_ReturnsRequiredError(string content)
        {
            var error = Assert.Single(Validate(content));

            Assert.Equal(string.Format(ErrorMessages.RequiredField, "Your tip"), error.ErrorMessage);
        }

        [Fact]
        public void Validation_TooShortTip_ReturnsLengthError()
        {
            var error = Assert.Single(Validate("Go early."));

            Assert.Equal(LengthMessage(), error.ErrorMessage);
        }

        [Fact]
        public void Validation_TooLongTip_ReturnsLengthError()
        {
            var error = Assert.Single(Validate(new string('a', ExpertTipValidationConstants.ContentMaxLength + 1)));

            Assert.Equal(LengthMessage(), error.ErrorMessage);
        }

        [Theory]
        [InlineData(ExpertTipValidationConstants.ContentMinLength)]
        [InlineData(ExpertTipValidationConstants.ContentMaxLength)]
        public void Validation_LengthOnTheBorder_HasNoErrors(int length)
        {
            Assert.Empty(Validate(new string('a', length)));
        }

        private static string LengthMessage()
        {
            return string.Format(
                ErrorMessages.ExpertTipContentLength,
                "Your tip",
                ExpertTipValidationConstants.ContentMaxLength,
                ExpertTipValidationConstants.ContentMinLength);
        }

        private static List<ValidationResult> Validate(string content)
        {
            var model = new ExpertTipFormModel { Content = content };
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            return results;
        }
    }
}
