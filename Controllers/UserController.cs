using GP.Models;
using GP.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Drawing;
using QRCoder; // for QR code generation
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
                .Where(e => e.IsApproved && !e.IsDeleted && e.Date > now);

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
                .FirstOrDefaultAsync(e => e.Id == id && e.IsApproved && !e.IsDeleted && e.Date > now);

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
            var user = await _context.Users.FindAsync(userId);

            var ticketType = await _context.TicketTypes.Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == dto.TicketTypeId);

            if (ticketType == null || ticketType.Quantity < dto.Quantity || !ticketType.Event.IsApproved)
                return BadRequest(new { message = "Invalid ticket or not enough available." });

            // Decrement tickets
            ticketType.Quantity -= dto.Quantity;

            // Create reservation & ticket
            var reservation = new Reservation
            {
                UserId = userId,
                TicketTypeId = ticketType.Id,
                EventId = ticketType.EventId,
                Quantity = dto.Quantity,
                ReservedDateTime = ticketType.Event.Date,
                TotalPrice = dto.Quantity * ticketType.Price,
                Status = "Confirmed",
            };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Create a ticket entry for each ticket (for PDF & QR code)
            for (int i = 0; i < dto.Quantity; i++)
            {
                var ticket = new Ticket
                {
                    ReservationId = reservation.Id,
                    QRCode = Guid.NewGuid().ToString(),
                    IsUsed = false
                };
                _context.Tickets.Add(ticket);
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ticket booked successfully.", reservationId = reservation.Id });
        }

        // 4. Generate/download PDF ticket for a reservation (each ticket)
        [HttpGet("ticket-pdf/{reservationId}")]
        public async Task<IActionResult> GetTicketPdf(int reservationId)
        {
            // 1. Find the reservation and all related info
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Event)
                .Include(r => r.TicketType)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return NotFound("Reservation not found.");

            // 2. Generate QR code as PNG byte[]
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode($"Ticket:{reservation.Id}", QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20); // size 20 pixels/module

            // 3. Generate PDF
            using (var pdfStream = new MemoryStream())
            {
                var pdf = new PdfDocument();
                var page = pdf.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                // Draw text info
                gfx.DrawString("Event Ticket", new XFont("Arial", 20, XFontStyle.Bold), XBrushes.Black, new XPoint(40, 40));
                gfx.DrawString($"Name: {reservation.User.UserName}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 80));
                gfx.DrawString($"Event: {reservation.Event.Name}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 110));
                gfx.DrawString($"Date: {reservation.Event.Date:yyyy-MM-dd HH:mm}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 140));
                gfx.DrawString($"Ticket Type: {reservation.TicketType.Name}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 170));
                gfx.DrawString($"Ticket ID: {reservation.Id}", new XFont("Arial", 14), XBrushes.Black, new XPoint(40, 200));

                // Draw QR code image
                using (var qrStream = new MemoryStream(qrCodeImage))
                {
                    var xImage = XImage.FromStream(() => qrStream);
                    gfx.DrawImage(xImage, page.Width - 200, 80, 120, 120); // Position/size as needed
                }

                pdf.Save(pdfStream, false);
                pdfStream.Position = 0;

                // Return as file download
                return File(pdfStream.ToArray(), "application/pdf", $"ticket_{reservation.Id}.pdf");
            }
        }


        // 5. My Tickets (with download PDF links)
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tickets = await _context.Reservations
                .Include(r => r.Event)
                .Include(r => r.TicketType)
                .Include(r => r.Tickets)
                .Where(r => r.UserId == userId && r.TicketTypeId != null)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    EventName = r.Event.Name,
                    EventDate = r.Event.Date,
                    TicketType = r.TicketType.Name,
                    Quantity = r.Quantity,
                    TotalPrice = r.TotalPrice,
                    Tickets = r.Tickets.Select(t => new
                    {
                        t.Id,
                        t.QRCode,
                        PdfUrl = $"/api/User/ticket-pdf/{r.Id}/{t.Id}"
                    })
                })
                .ToListAsync();

            return Ok(tickets);
        }
    }

    public class BuyTicketDto
    {
        public int TicketTypeId { get; set; }
        public int Quantity { get; set; }
    }
}
