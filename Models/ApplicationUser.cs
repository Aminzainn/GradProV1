using Microsoft.AspNetCore.Identity;

namespace GP.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string LastName { get; set; }
        public bool Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Event> OrganizedEvents { get; set; }
        public ICollection<Payment> Payments { get; set; }

      
    }
}
