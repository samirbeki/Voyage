using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Data.Models.Enums;
using Voyage.Web.ViewModels.Packing;
using Xunit;

namespace Voyage.Services.Tests
{
    public class PackingItemFormModelTests
    {
        [Fact]
        public void Validation_CorrectItem_HasNoErrors()
        {
            Assert.Empty(Validate(CreateModel("Sunglasses", 1, PackingCategory.Other)));
        }

        [Fact]
        public void NewModel_HasDefaultQuantityAndCategory()
        {
            var model = new PackingItemFormModel();

            Assert.Equal(1, model.Quantity);
            Assert.Equal(PackingCategory.Other, model.Category);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validation_MissingName_ReturnsRequiredError(string name)
        {
            var error = Assert.Single(Validate(CreateModel(name)));

            Assert.Equal(ErrorMessages.PackingItemNameRequired, error.ErrorMessage);
        }

        [Fact]
        public void Validation_TooShortName_ReturnsLengthError()
        {
            var error = Assert.Single(Validate(CreateModel("X")));

            Assert.Equal(NameLengthMessage(), error.ErrorMessage);
        }

        [Fact]
        public void Validation_TooLongName_ReturnsLengthError()
        {
            var name = new string('a', PackingItemValidationConstants.NameMaxLength + 1);

            var error = Assert.Single(Validate(CreateModel(name)));

            Assert.Equal(NameLengthMessage(), error.ErrorMessage);
        }

        [Fact]
        public void Validation_NameWithMaxLength_HasNoErrors()
        {
            var name = new string('a', PackingItemValidationConstants.NameMaxLength);

            Assert.Empty(Validate(CreateModel(name)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(100)]
        public void Validation_QuantityOutsideTheRange_ReturnsError(int quantity)
        {
            var error = Assert.Single(Validate(CreateModel("Socks", quantity)));

            var expected = string.Format(
                ErrorMessages.PackingItemQuantityRange,
                nameof(PackingItemFormModel.Quantity),
                PackingItemValidationConstants.QuantityMin,
                PackingItemValidationConstants.QuantityMax);

            Assert.Equal(expected, error.ErrorMessage);
        }

        [Theory]
        [InlineData(PackingItemValidationConstants.QuantityMin)]
        [InlineData(PackingItemValidationConstants.QuantityMax)]
        public void Validation_QuantityOnTheBorder_HasNoErrors(int quantity)
        {
            Assert.Empty(Validate(CreateModel("Socks", quantity)));
        }

        [Fact]
        public void Validation_UndefinedCategory_ReturnsError()
        {
            var error = Assert.Single(Validate(CreateModel("Socks", 1, (PackingCategory)42)));

            Assert.Equal(ErrorMessages.PackingItemCategoryInvalid, error.ErrorMessage);
        }

        private static string NameLengthMessage()
        {
            return string.Format(
                ErrorMessages.PackingItemNameLength,
                nameof(PackingItemFormModel.Name),
                PackingItemValidationConstants.NameMaxLength,
                PackingItemValidationConstants.NameMinLength);
        }

        private static List<ValidationResult> Validate(PackingItemFormModel model)
        {
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            return results;
        }

        private static PackingItemFormModel CreateModel(
            string name,
            int quantity = 1,
            PackingCategory category = PackingCategory.Clothing)
        {
            return new PackingItemFormModel
            {
                Name = name,
                Quantity = quantity,
                Category = category
            };
        }
    }
}
