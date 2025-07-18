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

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
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
    }
}
