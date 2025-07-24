using GP.Models;
using GP.Models.DTO;
using GP.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventManagerContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EventController(EventManagerContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // ✅ Add Event with Image, Documents, and Location
        [HttpPost("add-event")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> AddEventWithImage([FromForm] AddEventWithImageDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null) return Unauthorized();
            if (dto.Image == null)
                return BadRequest("Image is required");

            string imagesPath = Path.Combine(_env.WebRootPath, "images", "events");
            Directory.CreateDirectory(imagesPath);

            // Poster image
            string posterFileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.Image.FileName)}";
            string posterPath = Path.Combine(imagesPath, posterFileName);
            using (var stream = new FileStream(posterPath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            // Other docs
            string? securityClearanceUrl = await SaveDocAsync(dto.SecurityClearance, imagesPath, "security");
            string? publicLicenseFrontUrl = await SaveDocAsync(dto.PublicLicenseFront, imagesPath, "public_front");
            string? publicLicenseBackUrl = await SaveDocAsync(dto.PublicLicenseBack, imagesPath, "public_back");
            string? civilProtectionApprovalFrontUrl = await SaveDocAsync(dto.CivilProtectionApprovalFront, imagesPath, "civprot_front");
            string? civilProtectionApprovalBackUrl = await SaveDocAsync(dto.CivilProtectionApprovalBack, imagesPath, "civprot_back");
            string? eventInsuranceUrl = await SaveDocAsync(dto.EventInsurance, imagesPath, "insurance");

            var newEvent = new Event
            {
                Name = dto.Name,
                EventType = dto.EventType,
                StadiumName = dto.StadiumName,
                TeamA = dto.TeamA,
                TeamB = dto.TeamB,
                Performers = dto.Performers != null ? string.Join(", ", dto.Performers) : null,
                PlaceName = dto.PlaceName,
                Date = dto.Date,
                Description = dto.Description,
                ImageUrl = $"/images/events/{posterFileName}",
                CreatedByUserId = userId,
                IsTicketed = dto.TicketTypes != null && dto.TicketTypes.Count > 0,
                IsApproved = false,
                IsDeleted = false,
                SecurityClearanceUrl = securityClearanceUrl,
                PublicLicenseFrontUrl = publicLicenseFrontUrl,
                PublicLicenseBackUrl = publicLicenseBackUrl,
                CivilProtectionApprovalFrontUrl = civilProtectionApprovalFrontUrl,
                CivilProtectionApprovalBackUrl = civilProtectionApprovalBackUrl,
                EventInsuranceUrl = eventInsuranceUrl,
                TicketTypes = dto.TicketTypes?.Select(t => new TicketType
                {
                    Name = t.Name,
                    Price = t.Price,
                    Quantity = t.Quantity
                }).ToList(),
                // 👇 NEW: Location fields for Leaflet
                LocationAddress = dto.LocationAddress,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event created successfully", eventId = newEvent.Id });
        }

        // Helper to save docs
        private async Task<string?> SaveDocAsync(IFormFile? file, string folder, string label)
        {
            if (file == null) return null;
            string docFileName = $"{Guid.NewGuid()}_{label}_{Path.GetFileName(file.FileName)}";
            string docPath = Path.Combine(folder, docFileName);
            using (var stream = new FileStream(docPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/images/events/{docFileName}";
        }

        // ✅ Get Events Created by This Service Provider
        [HttpGet("my-events")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null) return Unauthorized();

            var events = await _context.Events
                .Include(e => e.TicketTypes)
                .Where(e => e.CreatedByUserId == userId && !e.IsDeleted)
                .Select(e => new MyEventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date,
                    Description = e.Description,
                    ImageUrl = e.ImageUrl,
                    EventType = e.EventType,
                    StadiumName = e.StadiumName,
                    TeamA = e.TeamA,
                    TeamB = e.TeamB,
                    Performers = e.Performers,
                    PlaceName = e.PlaceName,
                    IsApproved = e.IsApproved,
                    IsTicketed = e.IsTicketed,
                    SecurityClearanceUrl = e.SecurityClearanceUrl,
                    PublicLicenseFrontUrl = e.PublicLicenseFrontUrl,
                    PublicLicenseBackUrl = e.PublicLicenseBackUrl,
                    CivilProtectionApprovalFrontUrl = e.CivilProtectionApprovalFrontUrl,
                    CivilProtectionApprovalBackUrl = e.CivilProtectionApprovalBackUrl,
                    EventInsuranceUrl = e.EventInsuranceUrl,
                    TicketTypes = e.TicketTypes.Select(t => new TicketTypeDto
                    {
                        Name = t.Name,
                        Price = t.Price,
                        Quantity = t.Quantity
                    }).ToList(),
                    // 👇 NEW
                    LocationAddress = e.LocationAddress,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude
                })
                .ToListAsync();

            return Ok(events);
        }

        // ✅ Get Event Details By Id (for Service Provider)
        [HttpGet("{id}")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null) return Unauthorized();

            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .Where(e => e.Id == id && e.CreatedByUserId == userId && !e.IsDeleted)
                .Select(e => new MyEventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date,
                    Description = e.Description,
                    ImageUrl = e.ImageUrl,
                    EventType = e.EventType,
                    StadiumName = e.StadiumName,
                    TeamA = e.TeamA,
                    TeamB = e.TeamB,
                    Performers = e.Performers,
                    PlaceName = e.PlaceName,
                    IsApproved = e.IsApproved,
                    IsTicketed = e.IsTicketed,
                    SecurityClearanceUrl = e.SecurityClearanceUrl,
                    PublicLicenseFrontUrl = e.PublicLicenseFrontUrl,
                    PublicLicenseBackUrl = e.PublicLicenseBackUrl,
                    CivilProtectionApprovalFrontUrl = e.CivilProtectionApprovalFrontUrl,
                    CivilProtectionApprovalBackUrl = e.CivilProtectionApprovalBackUrl,
                    EventInsuranceUrl = e.EventInsuranceUrl,
                    TicketTypes = e.TicketTypes.Select(t => new TicketTypeDto
                    {
                        Name = t.Name,
                        Price = t.Price,
                        Quantity = t.Quantity
                    }).ToList(),
                    // 👇 NEW
                    LocationAddress = e.LocationAddress,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude
                }).FirstOrDefaultAsync();

            if (ev == null) return NotFound();

            return Ok(ev);
        }

        // ✅ Edit Event (add new or increase existing ticket types and docs)
        [HttpPut("edit-event/{id}")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> EditEvent(int id, [FromForm] AddEventWithImageDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null) return Unauthorized();

            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id && e.CreatedByUserId == userId && !e.IsDeleted);

            if (ev == null) return NotFound();

            string imagesPath = Path.Combine(_env.WebRootPath, "images", "events");
            Directory.CreateDirectory(imagesPath);

            // Update event info
            ev.Name = dto.Name ?? ev.Name;
            ev.EventType = dto.EventType ?? ev.EventType;
            ev.Date = dto.Date != DateTime.MinValue ? dto.Date : ev.Date;
            ev.Description = dto.Description ?? ev.Description;
            ev.StadiumName = dto.StadiumName ?? ev.StadiumName;
            ev.TeamA = dto.TeamA ?? ev.TeamA;
            ev.TeamB = dto.TeamB ?? ev.TeamB;
            ev.Performers = dto.Performers != null ? string.Join(", ", dto.Performers) : ev.Performers;
            ev.PlaceName = dto.PlaceName ?? ev.PlaceName;
            // 👇 NEW: Update location
            ev.LocationAddress = dto.LocationAddress ?? ev.LocationAddress;
            ev.Latitude = dto.Latitude ?? ev.Latitude;
            ev.Longitude = dto.Longitude ?? ev.Longitude;

            // Update images if uploaded
            if (dto.Image != null)
            {
                string posterFileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.Image.FileName)}";
                string posterPath = Path.Combine(imagesPath, posterFileName);
                using (var stream = new FileStream(posterPath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }
                ev.ImageUrl = $"/images/events/{posterFileName}";
            }

            if (dto.SecurityClearance != null)
                ev.SecurityClearanceUrl = await SaveDocAsync(dto.SecurityClearance, imagesPath, "security");
            if (dto.PublicLicenseFront != null)
                ev.PublicLicenseFrontUrl = await SaveDocAsync(dto.PublicLicenseFront, imagesPath, "public_front");
            if (dto.PublicLicenseBack != null)
                ev.PublicLicenseBackUrl = await SaveDocAsync(dto.PublicLicenseBack, imagesPath, "public_back");
            if (dto.CivilProtectionApprovalFront != null)
                ev.CivilProtectionApprovalFrontUrl = await SaveDocAsync(dto.CivilProtectionApprovalFront, imagesPath, "civprot_front");
            if (dto.CivilProtectionApprovalBack != null)
                ev.CivilProtectionApprovalBackUrl = await SaveDocAsync(dto.CivilProtectionApprovalBack, imagesPath, "civprot_back");
            if (dto.EventInsurance != null)
                ev.EventInsuranceUrl = await SaveDocAsync(dto.EventInsurance, imagesPath, "insurance");

            // Add to quantity if exists, else add new ticket type
            if (dto.TicketTypes != null && dto.TicketTypes.Count > 0)
            {
                foreach (var t in dto.TicketTypes)
                {
                    var existingTicket = ev.TicketTypes.FirstOrDefault(x => x.Name.ToLower() == t.Name.ToLower());
                    if (existingTicket != null)
                    {
                        existingTicket.Quantity += t.Quantity;
                        existingTicket.Price = t.Price;
                    }
                    else
                    {
                        ev.TicketTypes.Add(new TicketType
                        {
                            Name = t.Name,
                            Price = t.Price,
                            Quantity = t.Quantity
                        });
                    }
                }
                ev.IsTicketed = true;
            }

            // Set event back to pending after edit
            ev.IsApproved = false;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated. Awaiting approval." });
        }

        // ✅ Delete Event (soft delete)
        [HttpDelete("delete-event/{id}")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null) return Unauthorized();

            var ev = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.CreatedByUserId == userId && !e.IsDeleted);

            if (ev == null) return NotFound();

            ev.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event deleted." });
        }

        // 🎟️ User purchases ticket, decrements ticket type quantity
        [HttpPost("buy-ticket/{ticketTypeId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> BuyTicket(int ticketTypeId, [FromBody] int quantity)
        {
            var ticketType = await _context.TicketTypes.FindAsync(ticketTypeId);
            if (ticketType == null) return NotFound("Ticket type not found.");
            if (ticketType.Quantity < quantity) return BadRequest("Not enough tickets available.");

            ticketType.Quantity -= quantity;
            await _context.SaveChangesAsync();

            // TODO: Add reservation, payment, etc.

            return Ok(new { message = "Ticket purchased successfully." });
        }
    }
}
