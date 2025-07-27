namespace GP.Models.DTO
{
    public class ServiceProviderRequestDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string NationalIdFrontUrl { get; set; }
        public string NationalIdBackUrl { get; set; }
        public string HoldingIdUrl { get; set; }
        public string? StripePaymentLink { get; set; }
        public DateTime RequestedAt { get; set; }
        public bool? IsApproved { get; set; }
        public string? AdminNote { get; set; }
        public string Email { get; set; }
    }
}