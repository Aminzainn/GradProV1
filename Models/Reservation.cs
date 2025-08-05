using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace GP.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public int? TicketTypeId { get; set; } // Nullable for non-ticketed
        public int? EventId { get; set; }     // Nullable for ticketed
        public int Quantity { get; set; }
        public DateTime? ReservedDateTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } // Pending, Confirmed, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties
        public ApplicationUser User { get; set; }
        public TicketType TicketType { get; set; }
        public Event Event { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
        public Payment Payment { get; set; }
        public EventAvailability BookedSlot { get; set; }

        public int? PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public Place Place { get; set; }


    }
}

