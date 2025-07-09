using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace GP.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int EventTypeId { get; set; }
        public int PlaceId { get; set; }
        [ForeignKey("CreatedByUser")]
        public string CreatedByUserId { get; set; }
        public DateTime Date { get; set; }
        public bool IsTicketed { get; set; }
        public decimal? FixedPrice { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public EventType EventType { get; set; }
        public Place Place { get; set; }
        public ApplicationUser CreatedByUser { get; set; }
        public ICollection<TicketType> TicketTypes { get; set; }
        public ICollection<EventAvailability> AvailableSlots { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<EventParticipant> Participants { get; set; }
    }
}
