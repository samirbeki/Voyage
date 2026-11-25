using Voyage.Common.Utilities;
using Voyage.Data.Models.Enums;
using Voyage.Web.ViewModels.Enums;
using Xunit;

namespace Voyage.Services.Tests.Utilities
{
    public class EnumExtensionsTests
    {
        [Theory]
        [InlineData(FavoritesSorting.Recent, "Recently added")]
        [InlineData(FavoritesSorting.Oldest, "Oldest first")]
        [InlineData(FavoritesSorting.Name, "Name (A–Z)")]
        [InlineData(FavoritesSorting.Rating, "Top rated")]
        public void GetDisplayName_ValueWithDisplayAttribute_ReturnsTheDisplayName(FavoritesSorting value, string expected)
        {
            Assert.Equal(expected, value.GetDisplayName());
        }

        [Theory]
        [InlineData(PackingCategory.Clothing, "Clothing")]
        [InlineData(PackingCategory.Documents, "Documents")]
        [InlineData(PackingCategory.Other, "Other")]
        public void GetDisplayName_PackingCategory_ReturnsTheDisplayName(PackingCategory value, string expected)
        {
            Assert.Equal(expected, value.GetDisplayName());
        }

        [Theory]
        [InlineData(ExpenseCategory.Accommodation, "Accommodation")]
        [InlineData(ExpenseCategory.Food, "Food")]
        public void GetDisplayName_ValueWithoutDisplayAttribute_ReturnsTheEnumName(ExpenseCategory value, string expected)
        {
            Assert.Equal(expected, value.GetDisplayName());
        }

        [Fact]
        public void GetDisplayName_EnumWithoutAnyAttributes_ReturnsTheEnumName()
        {
            Assert.Equal("Ongoing", TripStatus.Ongoing.GetDisplayName());
        }

        [Fact]
        public void GetDisplayName_UndefinedValue_ReturnsTheNumber()
        {
            Assert.Equal("99", ((FavoritesSorting)99).GetDisplayName());
        }

        [Fact]
        public void GetDisplayName_EveryPackingCategory_HasANonEmptyName()
        {
            Assert.All(Enum.GetValues<PackingCategory>(), c => Assert.False(string.IsNullOrWhiteSpace(c.GetDisplayName())));
        }
    }
}
