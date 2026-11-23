using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Web.ViewModels.Enums;

namespace Voyage.Web.ViewModels.Favorites
{
    public class FavoritesQueryModel
    {
        [StringLength(
            SharedValidationConstants.SearchMaxLength,
            ErrorMessage = ErrorMessages.MaxLengthField)]
        public string? Search { get; set; }

        public FavoritesSorting Sorting { get; set; } = FavoritesSorting.Recent;

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
    }
}
