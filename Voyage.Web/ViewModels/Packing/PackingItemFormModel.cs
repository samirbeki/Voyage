using System.ComponentModel.DataAnnotations;
using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Packing
{
    public class PackingItemFormModel
    {
        [Required(ErrorMessage = ErrorMessages.PackingItemNameRequired)]
        [StringLength(
            ValidationConstants.PackingItemNameMaxLength,
            MinimumLength = ValidationConstants.PackingItemNameMinLength,
            ErrorMessage = ErrorMessages.PackingItemNameLength)]
        public string Name { get; set; } = null!;

        [Range(
            ValidationConstants.PackingItemQuantityMin,
            ValidationConstants.PackingItemQuantityMax,
            ErrorMessage = ErrorMessages.PackingItemQuantityRange)]
        public int Quantity { get; set; } = 1;

        [EnumDataType(
            typeof(PackingCategory),
            ErrorMessage = ErrorMessages.PackingItemCategoryInvalid)]
        public PackingCategory Category { get; set; } = PackingCategory.Other;
    }
}