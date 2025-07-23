// D:\iti .net course\graduation project\first codre\GP\Models\DTO\AddEventWithImageDto.cs

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
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

    // Images & Documents
    public IFormFile? Image { get; set; } // Main image (event poster)

    public IFormFile? SecurityClearance { get; set; }
    public IFormFile? PublicLicenseFront { get; set; }
    public IFormFile? PublicLicenseBack { get; set; }
    public IFormFile? CivilProtectionApprovalFront { get; set; }
    public IFormFile? CivilProtectionApprovalBack { get; set; }
    public IFormFile? EventInsurance { get; set; }
    public string? StripePaymentLink { get; set; }

    public string? SecurityClearanceUrl { get; set; }
    public string? PublicLicenseFrontUrl { get; set; }
    public string? PublicLicenseBackUrl { get; set; }
    public string? CivilProtectionApprovalFrontUrl { get; set; }
    public string? CivilProtectionApprovalBackUrl { get; set; }
    public string? EventInsuranceUrl { get; set; }

}
