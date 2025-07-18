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

        // 👇 الصور المرفوعة
        public IFormFile? Image { get; set; }
    }
}
