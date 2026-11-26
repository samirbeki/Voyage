using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Voyage.Common.Constants.Validation;

namespace Voyage.Data.Models
{
    public class ExpertTip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(ExpertTipValidationConstants.ContentMaxLength)]
        public string Content { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        [Required]
        public string AuthorId { get; set; } = null!;

        public IdentityUser Author { get; set; } = null!;

        public int DestinationId { get; set; }

        public Destination Destination { get; set; } = null!;
    }
}
