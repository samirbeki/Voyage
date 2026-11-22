using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Voyage.Common.Constants.Validation;
using Voyage.Data.Models.Enums;

namespace Voyage.Data.Models
{
    public class PackingItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(PackingItemValidationConstants.NameMaxLength)]
        public string Name { get; set; } = null!;

        [Range(
            PackingItemValidationConstants.QuantityMin,
            PackingItemValidationConstants.QuantityMax)]
        public int Quantity { get; set; } = 1;

        public PackingCategory Category { get; set; }

        public bool IsPacked { get; set; }

        public DateTime CreatedOn { get; set; }

        [ForeignKey(nameof(Trip))]
        public int TripId { get; set; }

        public Trip Trip { get; set; } = null!;
    }
}