using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;

namespace Voyage.Data.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Range(
            ReviewValidationConstants.RatingMin,
            ReviewValidationConstants.RatingMax,
            ErrorMessage = ErrorMessages.ReviewRatingRange)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(ReviewValidationConstants.CommentMaxLength)]
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