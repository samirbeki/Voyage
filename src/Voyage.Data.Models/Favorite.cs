using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Voyage.Data.Models
{
    public class Favorite
    {
        [Required]
        public string UserId { get; set; } = null!;

        public IdentityUser User { get; set; } = null!;

        public int DestinationId { get; set; }

        public Destination Destination { get; set; } = null!;

        public DateTime AddedOn { get; set; }
    }
}