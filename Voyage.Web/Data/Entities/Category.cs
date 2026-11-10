using System.ComponentModel.DataAnnotations;
using Voyage.Web.Common;

namespace Voyage.Web.Data.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.CategoryNameMaxLength)]
        public string Name { get; set; } = null!;

        [MaxLength(ValidationConstants.CategoryDescriptionMaxLength)]
        public string? Description { get; set; }

        public ICollection<Destination> Destinations { get; set; }
            = new HashSet<Destination>();
    }
}