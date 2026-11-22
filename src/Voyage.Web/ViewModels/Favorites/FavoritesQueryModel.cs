using System.ComponentModel.DataAnnotations;
using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Favorites
{
    public class FavoritesQueryModel
    {
        [StringLength(
            ValidationConstants.SearchMaxLength,
            ErrorMessage = ErrorMessages.MaxLengthField)]
        public string? Search { get; set; }

        public FavoritesSorting Sorting { get; set; } = FavoritesSorting.Recent;

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
    }
}