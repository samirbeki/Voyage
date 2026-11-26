using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants.Validation;

namespace Voyage.Data.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(TripValidationConstants.TitleMaxLength)]
        public string Title { get; set; } = null!;

        [MaxLength(TripValidationConstants.DescriptionMaxLength)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Precision(18, 2)]
        public decimal Budget { get; set; }

        [MaxLength(SharedValidationConstants.ImageUrlMaxLength)]
        public string? CoverImageUrl { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid? ShareToken { get; set; }

        [Required]
        [ForeignKey(nameof(Owner))]
        public string OwnerId { get; set; } = null!;

        public IdentityUser Owner { get; set; } = null!;

        public ICollection<TripDestination> TripDestinations { get; set; }
            = new HashSet<TripDestination>();

        public ICollection<Expense> Expenses { get; set; }
            = new HashSet<Expense>();

        public ICollection<PackingItem> PackingItems { get; set; }
            = new HashSet<PackingItem>();
    }
}