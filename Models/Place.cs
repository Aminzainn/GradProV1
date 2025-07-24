using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class Place
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public int MaxAttendees { get; set; }
        public int PlaceTypeId { get; set; }
        public decimal Price { get; set; }
        public bool IsApproved { get; set; } = false;
        public string? ImageUrl { get; set; }
        public string? SecurityClearanceUrl { get; set; }
        public string? OwnershipOrRentalContractUrl { get; set; }
        public string? NationalIdFrontUrl { get; set; }
        public string? NationalIdBackUrl { get; set; }
        public string? StripePaymentLink { get; set; }
        public PlaceType PlaceType { get; set; }
        public ICollection<Event> Events { get; set; }
        public string? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public ApplicationUser? CreatedByUser { get; set; }

        public ICollection<PlaceAvailability> Availabilities { get; set; } // Add this to Place.cs

    }
}
