using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class Place
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public int MaxAttendees { get; set; }
        public string PlaceTypeName { get; set; } // Directly store PlaceTypeName instead of PlaceTypeId
        public decimal Price { get; set; }
        public bool IsApproved { get; set; } = false;
        public string? ImageUrl { get; set; }
        public string? SecurityClearanceUrl { get; set; }
        public string? OwnershipOrRentalContractUrl { get; set; }
        public string? NationalIdFrontUrl { get; set; }
        public string? NationalIdBackUrl { get; set; }
        public string? StripePaymentLink { get; set; }
        public string? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public ApplicationUser? CreatedByUser { get; set; }

        public ICollection<Event> Events { get; set; }

        // Navigation property for PlaceType
        public PlaceType? PlaceType { get; set; }  // This should be the navigation property to PlaceType


        // New fields to handle Location using Leaflet
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string? AdminNote { get; set; }  // Add this property for rejection reason

        public ICollection<PlaceAvailability> Availabilities { get; set; } // Add this to Place.cs
    }
}
