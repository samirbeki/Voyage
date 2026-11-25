using Voyage.Common.Constants.Validation;
using Voyage.Data.Models.Enums;
using Voyage.Services.Templates;
using Xunit;

namespace Voyage.Services.Tests
{
    public class PackingTemplatesTests
    {
        [Fact]
        public void GetEssentials_ReturnsFourteenItems()
        {
            Assert.Equal(14, PackingTemplates.GetEssentials(5).Count);
        }

        [Fact]
        public void GetEssentials_ItemNamesAreUnique()
        {
            var names = PackingTemplates.GetEssentials(5).Select(i => i.Name).ToList();

            Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }

        [Fact]
        public void GetEssentials_NamesFitTheValidationLimits()
        {
            Assert.All(PackingTemplates.GetEssentials(5), i =>
                Assert.InRange(
                    i.Name.Length,
                    PackingItemValidationConstants.NameMinLength,
                    PackingItemValidationConstants.NameMaxLength));
        }

        [Theory]
        [InlineData(PackingCategory.Documents, 3)]
        [InlineData(PackingCategory.Electronics, 3)]
        [InlineData(PackingCategory.Toiletries, 3)]
        [InlineData(PackingCategory.Clothing, 3)]
        [InlineData(PackingCategory.Health, 2)]
        [InlineData(PackingCategory.Other, 0)]
        public void GetEssentials_HasExpectedItemsPerCategory(PackingCategory category, int expected)
        {
            Assert.Equal(expected, PackingTemplates.GetEssentials(5).Count(i => i.Category == category));
        }

        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(2, 2, 1)]
        [InlineData(3, 3, 2)]
        [InlineData(7, 7, 4)]
        [InlineData(10, 10, 5)]
        public void GetEssentials_ClothingQuantity_DependsOnDays(int days, int expectedDaily, int expectedShirts)
        {
            var items = PackingTemplates.GetEssentials(days);

            Assert.Equal(expectedDaily, items.Single(i => i.Name == "Underwear").Quantity);
            Assert.Equal(expectedDaily, items.Single(i => i.Name == "Socks (pairs)").Quantity);
            Assert.Equal(expectedShirts, items.Single(i => i.Name == "T-shirts").Quantity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void GetEssentials_ZeroOrNegativeDays_UsesMinimumQuantity(int days)
        {
            var items = PackingTemplates.GetEssentials(days);

            Assert.Equal(PackingItemValidationConstants.QuantityMin, items.Single(i => i.Name == "Underwear").Quantity);
            Assert.Equal(PackingItemValidationConstants.QuantityMin, items.Single(i => i.Name == "T-shirts").Quantity);
        }

        [Theory]
        [InlineData(99)]
        [InlineData(150)]
        [InlineData(int.MaxValue)]
        public void GetEssentials_TooManyDays_UsesMaximumQuantity(int days)
        {
            var items = PackingTemplates.GetEssentials(days);

            Assert.Equal(PackingItemValidationConstants.QuantityMax, items.Single(i => i.Name == "Underwear").Quantity);
            Assert.Equal(50, items.Single(i => i.Name == "T-shirts").Quantity);
        }

        [Fact]
        public void GetEssentials_ItemsOutsideClothing_HaveQuantityOne()
        {
            var items = PackingTemplates.GetEssentials(10)
                .Where(i => i.Category != PackingCategory.Clothing);

            Assert.All(items, i => Assert.Equal(1, i.Quantity));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(30)]
        [InlineData(365)]
        public void GetEssentials_AllQuantitiesAreValid(int days)
        {
            Assert.All(PackingTemplates.GetEssentials(days), i =>
                Assert.InRange(
                    i.Quantity,
                    PackingItemValidationConstants.QuantityMin,
                    PackingItemValidationConstants.QuantityMax));
        }
    }
}
