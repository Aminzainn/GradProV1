using GP.Models;
using GP.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QRCoder;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;
using System.Threading.Tasks;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class UserController : ControllerBase
    {
        private readonly EventManagerContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(EventManagerContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. List all approved, upcoming events (filter, search)
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents([FromQuery] string? eventType = null, [FromQuery] string? search = null)
        {
            var now = DateTime.Now;
            var query = _context.Events
                .Include(e => e.TicketTypes)
                .Where(e => e.IsApproved && e.Date > now);

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e => e.EventType.ToLower() == eventType.ToLower());

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.Name.ToLower().Contains(search.ToLower()));

            var events = await query
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.EventType,
                    e.Date,
                    e.ImageUrl,
                    e.Description,
                    TicketTypes = e.TicketTypes.Select(t => new {
                        t.Id,
                        t.Name,
                        t.Price,
                        t.Quantity
                    })
                })
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }

        // 2. Event details with tickets
        [HttpGet("event/{id}")]
        public async Task<IActionResult> GetEventDetails(int id)
        {
            var now = DateTime.Now;
            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsApproved && e.Date > now);

            if (ev == null) return NotFound();

            return Ok(new
            {
                ev.Id,
                ev.Name,
                ev.EventType,
                ev.Date,
                ev.ImageUrl,
                ev.Description,
                ev.TeamA,
                ev.TeamB,
                ev.StadiumName,
                ev.Performers,
                ev.PlaceName,
                ev.LocationAddress,
                ev.Latitude,
                ev.Longitude,
                TicketTypes = ev.TicketTypes.Select(t => new {
                    t.Id,
                    t.Name,
                    t.Price,
                    t.Quantity
                })
            });
        }

        // 3. Buy/pay for a ticket (this is called from the Checkout page for each ticket)
        [HttpPost("buy-ticket")]
        public async Task<IActionResult> BuyTicket([FromBody] BuyTicketDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ticketType = await _context.TicketTypes
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == dto.TicketTypeId);

            if (ticketType == null || !ticketType.Event.IsApproved || ticketType.Quantity < dto.Quantity)
                return BadRequest(new { message = "Invalid ticket or not enough tickets available." });

            // Decrement available ticket count
            ticketType.Quantity -= dto.Quantity;

            var boughtTickets = new List<UserTicket>();

            for (int i = 0; i < dto.Quantity; i++)
            {
                string qr = Guid.NewGuid().ToString();
                var userTicket = new UserTicket
                {
                    UserId = userId,
                    TicketTypeId = ticketType.Id,
                    EventId = ticketType.EventId,
                    PurchasedAt = DateTime.Now,
                    Status = "Confirmed",
                    QrCode = qr // unique per ticket
                };

                _context.UserTickets.Add(userTicket);
                boughtTickets.Add(userTicket);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Tickets purchased successfully!", tickets = boughtTickets.Select(t => new { t.Id, t.QrCode }) });
        }

        // 4. Generate/download PDF ticket for a reservation (each ticket)
        [HttpGet("ticket-pdf/{ticketId}")]
        public async Task<IActionResult> GetTicketPdf(int ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userTicket = await _context.UserTickets
                .Include(ut => ut.Event)
                .Include(ut => ut.TicketType)
                .FirstOrDefaultAsync(ut => ut.Id == ticketId && ut.UserId == userId);

            if (userTicket == null)
                return NotFound("Ticket not found.");

            // Generate QR code as PNG byte[]
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(userTicket.QrCode, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20); // size 20 pixels/module

            // Generate PDF
            using (var pdfStream = new MemoryStream())
            {
                var pdf = new PdfDocument();
                var page = pdf.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                // Draw text info
                gfx.DrawString("Event Ticket", new XFont("Arial", 20, XFontStyle.Bold), XBrushes.Black, new XPoint(40, 40));
                gfx.DrawString($"Name: {userId}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 80));
                gfx.DrawString($"Event: {userTicket.Event.Name}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 110));
                gfx.DrawString($"Date: {userTicket.Event.Date:yyyy-MM-dd HH:mm}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 140));
                gfx.DrawString($"Ticket Type: {userTicket.TicketType.Name}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 170));
                gfx.DrawString($"Ticket ID: {userTicket.Id}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 200));

                // Draw QR code image
                using (var qrStream = new MemoryStream(qrCodeImage))
                {
                    var xImage = XImage.FromStream(() => qrStream);
                    gfx.DrawImage(xImage, page.Width - 200, 80, 120, 120);
                }

                pdf.Save(pdfStream, false);
                pdfStream.Position = 0;

                return File(pdfStream.ToArray(), "application/pdf", $"ticket_{userTicket.Id}.pdf");
            }
        }

        // 5. My Tickets (with download PDF links)
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tickets = await _context.UserTickets
                .Include(ut => ut.TicketType)
                .Include(ut => ut.Event)
                .Where(ut => ut.UserId == userId)
                .Select(ut => new {
                    ut.Id,
                    ut.PurchasedAt,
                    ut.Status,
                    ut.QrCode,
                    EventName = ut.Event.Name,
                    EventDate = ut.Event.Date,
                    TicketType = ut.TicketType.Name
                }).ToListAsync();

            return Ok(tickets);
        }

        // List all approved places (with filter)
        [HttpGet("places")]
        public async Task<IActionResult> GetPlaces([FromQuery] string? placeType = null, [FromQuery] string? search = null)
        {
            var query = _context.Places
                .Where(p => p.IsApproved);

            if (!string.IsNullOrEmpty(placeType))
                query = query.Where(p => p.PlaceTypeName.ToLower() == placeType.ToLower());

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Location.ToLower().Contains(search.ToLower())); // adjust field to your model

            var places = await query
                .Select(p => new {
                    p.Id,
                    Location = p.Location,
                    p.PlaceTypeName,
                    p.Latitude,
                    p.Longitude,
                    p.ImageUrl
                    // add fields as needed from your Place model
                })
                .OrderBy(p => p.Location)
                .ToListAsync();

            return Ok(places);
        }

        // Details for one place + available dates
        [HttpGet("place/{id}")]
        public async Task<IActionResult> GetPlaceDetails(int id)
        {
            var place = await _context.Places
                .Include(p => p.Availabilities)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsApproved);

            if (place == null) return NotFound();

            var availabilities = place.Availabilities
                //.Where(a => a.Date >= DateTime.Today) // Uncomment if you have Date field
                .Select(a => new {
                    a.Id,
                    a.Date,
                    IsAvailable = true // if you don't have IsAvailable, assume all are available (or add logic)
                }).ToList();

            return Ok(new
            {
                place.Id,
                Location = place.Location,
                place.PlaceTypeName,
                place.Latitude,
                place.Longitude,
                place.ImageUrl,
                Availabilities = availabilities
            });
        }

        // Place reservation (cart) -- Update to match your model
        [HttpPost("reserve-place")]
        public async Task<IActionResult> ReservePlace([FromBody] ReservePlaceDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == dto.PlaceId && p.IsApproved);
            if (place == null) return BadRequest("Place not found.");

            // Optionally check if date is already reserved by any reservation
            var isDateTaken = await _context.Reservations
                .AnyAsync(r => r.PlaceId == dto.PlaceId && r.ReservedDateTime == dto.Date);
            if (isDateTaken)
                return BadRequest("Selected date is not available.");

            // Create reservation
            var reservation = new Reservation
            {
                UserId = userId,
                PlaceId = place.Id,
                ReservedDateTime = dto.Date,
                Status = "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservation added to cart!", reservationId = reservation.Id });
        }

        public class ReservePlaceDto
        {
            public int PlaceId { get; set; }
            public DateTime Date { get; set; }
        }

        [HttpPost("pay-reservation/{reservationId}")]
        public async Task<IActionResult> PayReservation(int reservationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Place)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null) return NotFound("Reservation not found.");

            reservation.Status = "Confirmed";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Reservation confirmed!" });
        }

        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = await _context.Reservations
                .Include(r => r.Place)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservedDateTime)
                .Select(r => new {
                    r.Id,
                    Date = r.ReservedDateTime,
                    r.Status,
                    PlaceName = r.Place.Location, // Use Location as PlaceName substitute
                    r.PlaceId
                }).ToListAsync();

            return Ok(reservations);
        }

        [HttpGet("reservation-pdf/{reservationId}")]
        public async Task<IActionResult> GetReservationPdf(int reservationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Place)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null)
                return NotFound("Reservation not found.");

            if (reservation.Place == null)
                return BadRequest("This reservation does not have an associated place. Please contact support.");

            using (var pdfStream = new MemoryStream())
            {
                var pdf = new PdfDocument();
                var page = pdf.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                gfx.DrawString("Place Reservation", new XFont("Arial", 20, XFontStyle.Bold), XBrushes.Black, new XPoint(40, 40));
                gfx.DrawString($"Place: {reservation.Place.Location}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 80));
                gfx.DrawString($"Date: {reservation.ReservedDateTime:yyyy-MM-dd}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 110));
                gfx.DrawString($"Status: {reservation.Status}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 140));
                gfx.DrawString($"Reservation ID: {reservation.Id}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 170));

                pdf.Save(pdfStream, false);
                pdfStream.Position = 0;
                return File(pdfStream.ToArray(), "application/pdf", $"reservation_{reservation.Id}.pdf");
            }
        }


        // BuyTicketDto for reference
        public class BuyTicketDto
        {
            public int TicketTypeId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
