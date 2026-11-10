using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Voyage.Web.Common;

namespace Voyage.Web.Data.Entities
{
    public class Destination
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.DestinationNameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(ValidationConstants.DestinationCountryMaxLength)]
        public string Country { get; set; } = null!;

        [Required]
        [MaxLength(ValidationConstants.DestinationDescriptionMaxLength)]
        public string Description { get; set; } = null!;

        [MaxLength(ValidationConstants.ImageUrlMaxLength)]
        public string? ImageUrl { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;
    }
}