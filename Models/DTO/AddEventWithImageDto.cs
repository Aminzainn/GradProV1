using System.ComponentModel.DataAnnotations;
using GP.Models.DTOs;

public class AddEventWithImageDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string EventType { get; set; } // Match, Concert, Other

    [Required]
    public DateTime Date { get; set; }

    public string? Description { get; set; }

    // Match specific
    public string? StadiumName { get; set; }
    public string? TeamA { get; set; }
    public string? TeamB { get; set; }

    // Concert & Other
    public string? PlaceName { get; set; }
    public List<string>? Performers { get; set; }

    // Tickets (up to 5 types)
    public List<TicketTypeDto>? TicketTypes { get; set; }

    public IFormFile? Image { get; set; }
}
