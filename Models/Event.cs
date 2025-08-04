// D:\iti .net course\graduation project\first codre\GP\Models\Event.cs

using System.ComponentModel.DataAnnotations;

namespace GP.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? TeamA { get; set; }
        public string? TeamB { get; set; }
        public string? StadiumName { get; set; }
        public string? Performers { get; set; }
        public string? PlaceName { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser? CreatedByUser { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsTicketed { get; set; } = false;
        public decimal? FixedPrice { get; set; }
        public List<TicketType>? TicketTypes { get; set; }

        public string StripeProductId { get; set; }
        public string? StripePaymentLink { get; set; }
        public string? AdminNote { get; set; }

        // Add image URLs for each extra document if needed
        public string? SecurityClearanceUrl { get; set; }
        public string? PublicLicenseFrontUrl { get; set; }
        public string? PublicLicenseBackUrl { get; set; }
        public string? CivilProtectionApprovalFrontUrl { get; set; }
        public string? CivilProtectionApprovalBackUrl { get; set; }
        public string? EventInsuranceUrl { get; set; }

        // Location fields
        public string? LocationAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

}
