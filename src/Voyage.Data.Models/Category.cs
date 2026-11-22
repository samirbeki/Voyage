using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;

namespace Voyage.Data.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(CategoryValidationConstants.NameMaxLength)]
        public string Name { get; set; } = null!;

        [MaxLength(CategoryValidationConstants.DescriptionMaxLength)]
        public string? Description { get; set; }

        public ICollection<Destination> Destinations { get; set; }
            = new HashSet<Destination>();
    }
}