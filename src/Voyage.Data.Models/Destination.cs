using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Voyage.Common.Constants.Validation;

namespace Voyage.Data.Models
{
    public class Destination
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(DestinationValidationConstants.NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(DestinationValidationConstants.CountryMaxLength)]
        public string Country { get; set; } = null!;

        [Required]
        [MaxLength(DestinationValidationConstants.DescriptionMaxLength)]
        public string Description { get; set; } = null!;

        [MaxLength(SharedValidationConstants.ImageUrlMaxLength)]
        public string? ImageUrl { get; set; }

        [Range(
            DestinationValidationConstants.LatitudeMin,
            DestinationValidationConstants.LatitudeMax)]
        public double? Latitude { get; set; }

        [Range(
            DestinationValidationConstants.LongitudeMin,
            DestinationValidationConstants.LongitudeMax)]
        public double? Longitude { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public ICollection<TripDestination> TripDestinations { get; set; }
            = new HashSet<TripDestination>();

        public ICollection<Review> Reviews { get; set; }
            = new HashSet<Review>();

        public ICollection<Favorite> Favorites { get; set; }
            = new HashSet<Favorite>();

        public ICollection<ExpertTip> ExpertTips { get; set; }
            = new HashSet<ExpertTip>();
    }
}