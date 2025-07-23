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
        public string? TeamA { get; set; }
        public string? TeamB { get; set; }
        public string? StadiumName { get; set; }
        public string? Performers { get; set; }
        public string? PlaceName { get; set; }
        public bool IsApproved { get; set; }

        // Document URLs
        public string? SecurityClearanceUrl { get; set; }
        public string? PublicLicenseFrontUrl { get; set; }
        public string? PublicLicenseBackUrl { get; set; }
        public string? CivilProtectionApprovalFrontUrl { get; set; }
        public string? CivilProtectionApprovalBackUrl { get; set; }
        public string? EventInsuranceUrl { get; set; }
    }

}
