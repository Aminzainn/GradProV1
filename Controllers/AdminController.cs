using GP.Models;
using GP.Models.DTO;
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

        // Get pending events with documents and location info
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
                    e.AdminNote,
                    // 👇 Location fields
                    e.LocationAddress,
                    e.Latitude,
                    e.Longitude
                })
                .ToListAsync();

            return Ok(pendingEvents);
        }

        // Approve event
        [HttpPost("approve-event/{id}")]
        public async Task<IActionResult> ApproveEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null || ev.IsDeleted)
                return NotFound(new { message = "Event not found." });

            ev.IsApproved = true;
            ev.AdminNote = null;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event approved." });
        }

        // Reject event
        [HttpPost("reject-event/{id}")]
        public async Task<IActionResult> RejectEvent(int id, [FromBody] string adminNote)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null || ev.IsDeleted)
                return NotFound(new { message = "Event not found." });

            ev.IsApproved = false;
            ev.AdminNote = adminNote;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event rejected." });
        }

        // Get all pending and approved places
        [HttpGet("places")]
        public async Task<IActionResult> GetPlaces([FromQuery] bool? isApproved = null)
        {
            var query = _context.Places.Include(p => p.PlaceType).AsQueryable();

            // Filter places based on approval status
            if (isApproved.HasValue)
                query = query.Where(p => p.IsApproved == isApproved.Value);

            // Fetch and return the list of places with relevant details
            var places = await query.Select(p => new MyPlaceDto
            {
                Id = p.Id,
                Location = p.Location,
                MaxAttendees = p.MaxAttendees,
                PlaceTypeName = p.PlaceType.Name,  // Fetching PlaceType name as string
                IsApproved = p.IsApproved,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                SecurityClearanceUrl = p.SecurityClearanceUrl,
                OwnershipOrRentalContractUrl = p.OwnershipOrRentalContractUrl,
                NationalIdFrontUrl = p.NationalIdFrontUrl,
                NationalIdBackUrl = p.NationalIdBackUrl,
                StripePaymentLink = p.StripePaymentLink
            }).ToListAsync();

            return Ok(places);
        }


        // Approve place
        [HttpPost("approve-place/{id}")]
        public async Task<IActionResult> ApprovePlace(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place == null)
                return NotFound(new { message = "Place not found." });

            place.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Place approved." });
        }

        // Reject place
        [HttpPost("reject-place/{id}")]
        public async Task<IActionResult> RejectPlace(int id, [FromBody] string adminNote)
        {
            var place = await _context.Places.FindAsync(id);
            if (place == null)
                return NotFound(new { message = "Place not found." });

            place.IsApproved = false;
            //place.AdminNote = adminNote;  // Saving rejection reason
            await _context.SaveChangesAsync();

            return Ok(new { message = "Place rejected." });
        }
    }
}
