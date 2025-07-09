namespace GP.Models
{
    public class ParticipantType
    {
        public int Id { get; set; }
        public string Name { get; set; } // Player, Artist, Coach

        public ICollection<EventParticipant> EventParticipants { get; set; }
    }
}
