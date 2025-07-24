namespace GP.Models.DTO
{
    public class MyPlaceDto
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public int MaxAttendees { get; set; }
        public string PlaceTypeName { get; set; }
        public bool IsApproved { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? SecurityClearanceUrl { get; set; }
        public string? OwnershipOrRentalContractUrl { get; set; }
        public string? NationalIdFrontUrl { get; set; }
        public string? NationalIdBackUrl { get; set; }
        public string? StripePaymentLink { get; set; }
    }
}
