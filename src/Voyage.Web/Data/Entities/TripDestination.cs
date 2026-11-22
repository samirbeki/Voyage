using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Voyage.Web.Data.Entities
{
    [PrimaryKey(nameof(TripId), nameof(DestinationId))]
    public class TripDestination
    {
        [ForeignKey(nameof(Trip))]
        public int TripId { get; set; }

        public Trip Trip { get; set; } = null!;

        [ForeignKey(nameof(Destination))]
        public int DestinationId { get; set; }

        public Destination Destination { get; set; } = null!;
    }
}