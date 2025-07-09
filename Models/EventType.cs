namespace GP.Models
{
    public class EventType
    {
        public int Id { get; set; }
        public string Name { get; set; } // Concert, Match, Workshop

        public ICollection<Event> Events { get; set; }
    }
}
