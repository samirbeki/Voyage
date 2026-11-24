using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Voyage.Common.Messages;
using Voyage.Data.Models.Enums;
using Voyage.Web.ViewModels.Expenses;
using Xunit;

namespace Voyage.Services.Tests
{
    public class ExpenseFormViewModelTests
    {
        [Fact]
        public void Validation_CorrectExpense_HasNoErrors()
        {
            Assert.Empty(Validate(CreateModel("Hotel, 3 nights", 246m)));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-5")]
        [InlineData("1000000.01")]
        public void Validation_AmountOutsideTheAllowedRange_ReturnsError(string amount)
        {
            var errors = Validate(CreateModel("Hotel", decimal.Parse(amount, CultureInfo.InvariantCulture)));

            var error = Assert.Single(errors);
            Assert.Equal(ErrorMessages.ExpenseAmountRange, error.ErrorMessage);
        }

        [Fact]
        public void Validation_SmallestAllowedAmount_HasNoErrors()
        {
            Assert.Empty(Validate(CreateModel("Coffee", 0.01m)));
        }

        [Fact]
        public void Validation_MissingAmount_ReturnsError()
        {
            var error = Assert.Single(Validate(CreateModel("Taxi", null)));

            Assert.Contains(nameof(ExpenseFormViewModel.Amount), error.MemberNames);
        }

        [Fact]
        public void Validation_TooShortTitle_ReturnsError()
        {
            var error = Assert.Single(Validate(CreateModel("A", 10m)));

            Assert.Equal(ErrorMessages.ExpenseTitleLength, error.ErrorMessage);
        }

        private static List<ValidationResult> Validate(ExpenseFormViewModel model)
        {
            var results = new List<ValidationResult>();
            var originalCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }

            return results;
        }

        private static ExpenseFormViewModel CreateModel(string title, decimal? amount)
        {
            return new ExpenseFormViewModel
            {
                TripId = 1,
                Title = title,
                Amount = amount,
                Category = ExpenseCategory.Accommodation,
                Date = DateTime.Today
            };
        }
    }
}
