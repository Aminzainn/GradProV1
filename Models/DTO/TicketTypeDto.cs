using GP.Models.DTOs;

namespace GP.Models.DTOs

{
    public class TicketTypeDto
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
