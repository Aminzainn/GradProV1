using GP.Models;

public class ServiceProviderRequest
{
    public int Id { get; set; }
    public string UserId { get; set; } // FK to ApplicationUser
    public string NationalIdFrontUrl { get; set; }
    public string NationalIdBackUrl { get; set; }
    public string HoldingIdUrl { get; set; }
    public string? StripePaymentLink { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public bool? IsApproved { get; set; } // null: pending, true: approved, false: rejected
    public string? AdminNote { get; set; }

    public ApplicationUser User { get; set; }
}
