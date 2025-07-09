namespace GP.Models
{
    public class Place
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public int MaxAttendees { get; set; }
        public int PlaceTypeId { get; set; }

        public PlaceType PlaceType { get; set; }
        public ICollection<Event> Events { get; set; }
    }
}
