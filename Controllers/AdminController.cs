using GP.Models;
using GP.Models.DTO;
using GP.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly EventManagerContext _context;

        public AdminController(EventManagerContext context)
        {
            _context = context;
        }

        // ✅ GET: عرض الأحداث غير المعتمدة
        [HttpGet("pending-events")]
        public async Task<IActionResult> GetPendingEvents()
        {
            var pendingEvents = await _context.Events
                .Where(e => !e.IsApproved && !e.IsDeleted)
                .Select(e => new MyEventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventType = e.EventType,
                    Date = e.Date,
                    ImageUrl = e.ImageUrl,
                    IsTicketed = e.IsTicketed,
                    FixedPrice = e.FixedPrice,
                    TeamA = e.TeamA,
                    TeamB = e.TeamB,
                    StadiumName = e.StadiumName,
                    Performers = e.Performers,
                    PlaceName = e.PlaceName
                })
                .ToListAsync();

            return Ok(pendingEvents);
        }

        // ✅ POST: الموافقة على حدث معين
        [HttpPost("approve-event/{id}")]
        public async Task<IActionResult> ApproveEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null || ev.IsDeleted)
                return NotFound("Event not found.");

            ev.IsApproved = true;

            await _context.SaveChangesAsync();

            return Ok("Event approved.");
        }
    }
}
