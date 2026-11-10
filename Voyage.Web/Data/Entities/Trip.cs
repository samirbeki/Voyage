using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voyage.Web.Common;

namespace Voyage.Web.Data.Entities
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.TripTitleMaxLength)]
        public string Title { get; set; } = null!;

        [MaxLength(ValidationConstants.TripDescriptionMaxLength)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Precision(18, 2)]
        public decimal Budget { get; set; }

        [MaxLength(ValidationConstants.ImageUrlMaxLength)]
        public string? CoverImageUrl { get; set; }

        public DateTime CreatedOn { get; set; }

        [Required]
        [ForeignKey(nameof(Owner))]
        public string OwnerId { get; set; } = null!;

        public IdentityUser Owner { get; set; } = null!;

        public ICollection<TripDestination> TripDestinations { get; set; }
            = new HashSet<TripDestination>();
    }
}