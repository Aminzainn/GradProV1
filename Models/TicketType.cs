using GP.Models;
public class TicketType
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public bool IsDeleted { get; set; } = false;
    public string StripePriceId { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; }
}
