using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class Payment
    {
        public int Id { get; set; }
        [ForeignKey("Reservation")]
        public int ReservationId { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // Visa, Fawry, Cash
        public string PaymentStatus { get; set; } // Success, Failed, Pending
        public string TransactionRef { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; }

        public Reservation Reservation { get; set; }
        public ApplicationUser User { get; set; }

    }
}
