namespace GP.Models
{
    public class EventAvailability
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public bool IsBooked { get; set; } = false;

        public Event Event { get; set; }
        public Reservation Reservation { get; set; }
    }
}

