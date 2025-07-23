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

        // Get pending events with documents
        [HttpGet("pending-events")]
        public async Task<IActionResult> GetPendingEvents()
        {
            var pendingEvents = await _context.Events
                .Where(e => !e.IsApproved && !e.IsDeleted)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.EventType,
                    e.Date,
                    e.ImageUrl,
                    e.SecurityClearanceUrl,
                    e.PublicLicenseFrontUrl,
                    e.PublicLicenseBackUrl,
                    e.CivilProtectionApprovalFrontUrl,
                    e.CivilProtectionApprovalBackUrl,
                    e.EventInsuranceUrl,
                    e.StripePaymentLink,
                    e.Description,
                    e.TeamA,
                    e.TeamB,
                    e.StadiumName,
                    e.Performers,
                    e.PlaceName,
                    e.IsTicketed,
                    e.FixedPrice,
                    e.AdminNote
                })
                .ToListAsync();

            return Ok(pendingEvents);
        }

        // Approve event (same as before)
        [HttpPost("approve-event/{id}")]
        public async Task<IActionResult> ApproveEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null || ev.IsDeleted)
                return NotFound(new { message = "Event not found." });

            ev.IsApproved = true;
            ev.AdminNote = null;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event approved." });  // <- return JSON
        }

        // Reject event
        [HttpPost("reject-event/{id}")]
        public async Task<IActionResult> RejectEvent(int id, [FromBody] string adminNote)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null || ev.IsDeleted)
                return NotFound("Event not found.");

            ev.IsApproved = false;
            ev.AdminNote = adminNote;
            await _context.SaveChangesAsync();
            return Ok("Event rejected.");
        }


    }
}
