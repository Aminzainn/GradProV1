namespace GP.Models.DTO
{
    public class MyPlaceDto
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public int MaxAttendees { get; set; }
        public string PlaceTypeName { get; set; }
        public bool IsApproved { get; set; }
        public string? ImageUrl { get; set; }
    }
}
