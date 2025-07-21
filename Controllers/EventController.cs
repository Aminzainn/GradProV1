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

        // ✅ Add Event with Image
        [HttpPost("add-event")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> AddEventWithImage([FromForm] AddEventWithImageDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null) return Unauthorized();
            if (dto.Image == null)
                return BadRequest("Image is required");

            string extension = Path.GetExtension(dto.Image.FileName);
            string fileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(dto.Image.FileName).Replace(" ", "_")}{extension}";

            string filePath = Path.Combine(_env.WebRootPath, "images", "events", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

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
                ImageUrl = $"/images/events/{fileName}",
                CreatedByUserId = userId,
                IsTicketed = dto.TicketTypes != null && dto.TicketTypes.Count > 0,
                IsApproved = false,
                IsDeleted = false,
                TicketTypes = dto.TicketTypes?.Select(t => new TicketType
                {
                    Name = t.Name,
                    Price = t.Price,
                    Quantity = t.Quantity
                }).ToList()
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event created successfully", eventId = newEvent.Id });
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
                    TicketTypes = e.TicketTypes.Select(t => new TicketTypeDto
                    {
                        Name = t.Name,
                        Price = t.Price,
                        Quantity = t.Quantity
                    }).ToList()
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
                    TicketTypes = e.TicketTypes.Select(t => new TicketTypeDto
                    {
                        Name = t.Name,
                        Price = t.Price,
                        Quantity = t.Quantity
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (ev == null) return NotFound();

            return Ok(ev);
        }

        // ✅ Edit Event (by Service Provider) - Will set IsApproved = false after edit
        [HttpPut("edit-event/{id}")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> EditEvent(int id, [FromForm] AddEventWithImageDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null) return Unauthorized();

            var ev = await _context.Events.Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id && e.CreatedByUserId == userId && !e.IsDeleted);

            if (ev == null) return NotFound();

            // Update properties
            ev.Name = dto.Name ?? ev.Name;
            ev.EventType = dto.EventType ?? ev.EventType;
            ev.Date = dto.Date != DateTime.MinValue ? dto.Date : ev.Date;
            ev.Description = dto.Description ?? ev.Description;
            ev.StadiumName = dto.StadiumName ?? ev.StadiumName;
            ev.TeamA = dto.TeamA ?? ev.TeamA;
            ev.TeamB = dto.TeamB ?? ev.TeamB;
            ev.Performers = dto.Performers != null ? string.Join(", ", dto.Performers) : ev.Performers;
            ev.PlaceName = dto.PlaceName ?? ev.PlaceName;

            // لو الصورة اتغيرت
            if (dto.Image != null)
            {
                string extension = Path.GetExtension(dto.Image.FileName);
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(dto.Image.FileName).Replace(" ", "_")}{extension}";
                string filePath = Path.Combine(_env.WebRootPath, "images", "events", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }
                ev.ImageUrl = $"/images/events/{fileName}";
            }

            // تحديث التذاكر
            if (dto.TicketTypes != null && dto.TicketTypes.Count > 0)
            {
                _context.TicketTypes.RemoveRange(ev.TicketTypes);
                ev.TicketTypes = dto.TicketTypes.Select(t => new TicketType
                {
                    Name = t.Name,
                    Price = t.Price,
                    Quantity = t.Quantity
                }).ToList();
                ev.IsTicketed = true;
            }

            // أي تعديل يرجع الحدث لـ Pending
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
    }
}
