using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class PlaceAvailability
    {
        public int Id { get; set; }

        public int PlaceId { get; set; }
        public Place Place { get; set; }

        public DateTime Date { get; set; } // The day to block (yyyy-MM-dd)

        // Optional: add time slots if you want partial-day bookings
        // public TimeSpan? FromTime { get; set; }
        // public TimeSpan? ToTime { get; set; }

        public string? Note { get; set; } // Reason for block (maintenance, offline reservation, etc.)
        public bool IsBlocked { get; set; } = true; // If you want to allow future support for 'available' vs 'unavailable'
    }
}
