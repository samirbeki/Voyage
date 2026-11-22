using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Packing
{
    public class PackingItemFormModel
    {
        [Required(ErrorMessage = ErrorMessages.PackingItemNameRequired)]
        [StringLength(
            PackingItemValidationConstants.NameMaxLength,
            MinimumLength = PackingItemValidationConstants.NameMinLength,
            ErrorMessage = ErrorMessages.PackingItemNameLength)]
        public string Name { get; set; } = null!;

        [Range(
            PackingItemValidationConstants.QuantityMin,
            PackingItemValidationConstants.QuantityMax,
            ErrorMessage = ErrorMessages.PackingItemQuantityRange)]
        public int Quantity { get; set; } = 1;

        [EnumDataType(
            typeof(PackingCategory),
            ErrorMessage = ErrorMessages.PackingItemCategoryInvalid)]
        public PackingCategory Category { get; set; } = PackingCategory.Other;
    }
}