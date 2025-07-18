using GP.Models.DTOs;
using GP.Models;

namespace GP.Models.DTO
{
    public class MyEventDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsTicketed { get; set; }

        public decimal? FixedPrice { get; set; }

        public string? Description { get; set; }

        public List<TicketTypeDto> TicketTypes { get; set; } = new List<TicketTypeDto>();

        // ✅ Match
        public string? TeamA { get; set; }
        public string? TeamB { get; set; }
        public string? StadiumName { get; set; }

        // ✅ Concert
        public string? Performers { get; set; }

        // ✅ Other
        public string? PlaceName { get; set; }
    }
}
