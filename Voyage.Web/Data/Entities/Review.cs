using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Voyage.Web.Common;

namespace Voyage.Web.Data.Entities
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Range(
            ValidationConstants.ReviewRatingMin,
            ValidationConstants.ReviewRatingMax,
            ErrorMessage = ErrorMessages.ReviewRatingRange)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(ValidationConstants.ReviewCommentMaxLength)]
        public string Comment { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsHidden { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public string AuthorId { get; set; } = null!;

        public IdentityUser Author { get; set; } = null!;

        public int DestinationId { get; set; }

        public Destination Destination { get; set; } = null!;
    }
}