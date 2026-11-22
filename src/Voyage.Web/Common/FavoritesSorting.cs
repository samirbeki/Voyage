using System.ComponentModel.DataAnnotations;

namespace Voyage.Web.Common
{
    public enum FavoritesSorting
    {
        [Display(Name = "Recently added")]
        Recent = 0,

        [Display(Name = "Oldest first")]
        Oldest = 1,

        [Display(Name = "Name (A–Z)")]
        Name = 2,

        [Display(Name = "Top rated")]
        Rating = 3
    }
}