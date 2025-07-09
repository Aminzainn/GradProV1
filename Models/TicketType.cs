using System.Net.Sockets;

namespace GP.Models
{
    public class TicketType
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } // VIP, Regular
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public Event Event { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
