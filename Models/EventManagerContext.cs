using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GP.Models
{
    public class EventManagerContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public EventManagerContext(DbContextOptions<EventManagerContext> options)
            : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<PlaceType> PlaceTypes { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<EventAvailability> EventAvailabilities { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<ParticipantType> ParticipantTypes { get; set; }
        public DbSet<PlaceAvailability> PlaceAvailabilities { get; set; }

        public DbSet<ServiceProviderRequest> ServiceProviderRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Remove the foreign key relationship for PlaceTypeId and use PlaceTypeName instead
            builder.Entity<Place>()
                .Property(p => p.PlaceTypeName)
                .IsRequired(); // Ensure the PlaceTypeName is required for the frontend dropdown

            // Latitude and Longitude for storing location coordinates
            builder.Entity<Place>()
                .Property(p => p.Latitude)
                .HasColumnType("decimal(18, 10)");

            builder.Entity<Place>()
                .Property(p => p.Longitude)
                .HasColumnType("decimal(18, 10)");

            builder.Entity<PlaceAvailability>()
                .HasOne(pa => pa.Place)
                .WithMany(p => p.Availabilities)
                .HasForeignKey(pa => pa.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Reservation>()
                .HasOne(r => r.BookedSlot)
                .WithOne(ea => ea.Reservation)
                .HasForeignKey<EventAvailability>(ea => ea.Id);

            builder.Entity<Payment>()
                .HasOne(p => p.Reservation)
                .WithOne(r => r.Payment)
                .HasForeignKey<Reservation>(r => r.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Event>()
                .HasOne(e => e.CreatedByUser)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Place>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId);

            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId);

            builder.Entity<TicketType>()
                .HasOne(t => t.Event)
                .WithMany(e => e.TicketTypes)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔥 Soft Delete Filters
            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<EventAvailability>().HasQueryFilter(e => !e.Event.IsDeleted);
            builder.Entity<EventParticipant>().HasQueryFilter(e => !e.Event.IsDeleted);
            builder.Entity<Reservation>().HasQueryFilter(r => !r.Event.IsDeleted);
            builder.Entity<TicketType>().HasQueryFilter(tt => !tt.Event.IsDeleted);
            builder.Entity<Payment>().HasQueryFilter(p => !p.Reservation.Event.IsDeleted);
            builder.Entity<Ticket>().HasQueryFilter(t => !t.Reservation.Event.IsDeleted);

            // 🔢 Decimal Precision for financial fields
            builder.Entity<Event>()
                .Property(e => e.FixedPrice)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Reservation>()
                .Property(r => r.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<TicketType>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);

            // ✅ Payment-Reservation relationship again (to avoid delete errors)
            builder.Entity<Reservation>()
                .HasOne(r => r.Payment)
                .WithOne(p => p.Reservation)
                .HasForeignKey<Payment>(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
