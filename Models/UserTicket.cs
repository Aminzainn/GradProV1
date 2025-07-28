namespace GP.Models
{
    public class UserTicket
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TicketTypeId { get; set; }
        public int EventId { get; set; }
        public DateTime PurchasedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Confirmed";
        public string QrCode { get; set; } // can be a unique code (Guid)

        public TicketType TicketType { get; set; }
        public Event Event { get; set; }
    }
}
