namespace GP.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string QRCode { get; set; } = Guid.NewGuid().ToString();
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Reservation Reservation { get; set; }
    }
}
