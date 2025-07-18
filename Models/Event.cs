using System.ComponentModel.DataAnnotations;

namespace GP.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string EventType { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string? TeamA { get; set; }
        public string? TeamB { get; set; }
        public string? StadiumName { get; set; }

        public string? Performers { get; set; }

        public string? PlaceName { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        public ApplicationUser? CreatedByUser { get; set; }

        public bool IsApproved { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public bool IsTicketed { get; set; } = false;

        public decimal? FixedPrice { get; set; }

        public List<TicketType>? TicketTypes { get; set; }
    }
}
