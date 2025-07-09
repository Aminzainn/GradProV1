namespace GP.Models
{
    public class EventParticipant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int EventId { get; set; }
        public int ParticipantTypeId { get; set; }

        public Event Event { get; set; }
        public ParticipantType ParticipantType { get; set; }
    }
}
