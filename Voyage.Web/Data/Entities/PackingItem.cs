using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Voyage.Web.Common;

namespace Voyage.Web.Data.Entities
{
    public class PackingItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.PackingItemNameMaxLength)]
        public string Name { get; set; } = null!;

        [Range(
            ValidationConstants.PackingItemQuantityMin,
            ValidationConstants.PackingItemQuantityMax)]
        public int Quantity { get; set; } = 1;

        public PackingCategory Category { get; set; }

        public bool IsPacked { get; set; }

        public DateTime CreatedOn { get; set; }

        [ForeignKey(nameof(Trip))]
        public int TripId { get; set; }

        public Trip Trip { get; set; } = null!;
    }
}