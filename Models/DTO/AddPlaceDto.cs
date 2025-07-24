using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GP.Models.DTO
{
    public class AddPlaceDto
    {
        [Required]
        public string Location { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxAttendees must be at least 1.")]
        public int MaxAttendees { get; set; }

        [Required]
        public int PlaceTypeId { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string? StripePaymentLink { get; set; }

        public IFormFile? Image { get; set; }
        public IFormFile? SecurityClearance { get; set; }
        public IFormFile? OwnershipOrRentalContract { get; set; }
        public IFormFile? NationalIdFront { get; set; }
        public IFormFile? NationalIdBack { get; set; }

        // For updating/editing, in case you want to keep URLs as well
        public string? ImageUrl { get; set; }
        public string? SecurityClearanceUrl { get; set; }
        public string? OwnershipOrRentalContractUrl { get; set; }
        public string? NationalIdFrontUrl { get; set; }
        public string? NationalIdBackUrl { get; set; }
    }
}
