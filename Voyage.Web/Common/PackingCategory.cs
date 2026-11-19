using System.ComponentModel.DataAnnotations;

namespace Voyage.Web.Common
{
    public enum PackingCategory
    {
        [Display(Name = "Clothing")]
        Clothing = 0,

        [Display(Name = "Toiletries")]
        Toiletries = 1,

        [Display(Name = "Electronics")]
        Electronics = 2,

        [Display(Name = "Documents")]
        Documents = 3,

        [Display(Name = "Health")]
        Health = 4,

        [Display(Name = "Other")]
        Other = 5
    }
}