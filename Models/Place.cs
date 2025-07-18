using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class Place
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public int MaxAttendees { get; set; }
        public int PlaceTypeId { get; set; }

        public bool IsApproved { get; set; } = false;

        public string? ImageUrl { get; set; }


        public PlaceType PlaceType { get; set; }
        public ICollection<Event> Events { get; set; }

        public string? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public ApplicationUser? CreatedByUser { get; set; }

    }
}
