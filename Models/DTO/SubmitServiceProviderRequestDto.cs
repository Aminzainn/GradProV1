namespace GP.Models.DTO
{
    public class SubmitServiceProviderRequestDto
    {
        public IFormFile NationalIdFront { get; set; }
        public IFormFile NationalIdBack { get; set; }
        public IFormFile HoldingId { get; set; }
        public string? StripePaymentLink { get; set; }
    }

}
