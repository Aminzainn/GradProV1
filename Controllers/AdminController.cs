using GP.Models;
using GP.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GP.Models;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly EventManagerContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(EventManagerContext context , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }


        // Get all events (approved and pending) with filtering by eventType (optional)
        [HttpGet("all-events")]
        public async Task<IActionResult> GetAllEvents([FromQuery] string? eventType = null)
        {
            var query = _context.Events
                .Where(e => !e.IsDeleted);

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e => e.EventType.ToLower() == eventType.ToLower());

            var events = await query
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.EventType,
                    e.Date,
                    e.ImageUrl,
                    e.Description,
                    e.TeamA,
                    e.TeamB,
                    e.StadiumName,
                    e.Performers,
                    e.PlaceName,
                    e.IsTicketed,
                    e.FixedPrice,
                    e.AdminNote,
                    e.IsApproved,
                    e.Latitude,
                    e.Longitude,
                    e.SecurityClearanceUrl,
                    e.PublicLicenseFrontUrl,
                    e.PublicLicenseBackUrl,
                    e.CivilProtectionApprovalFrontUrl,
                    e.CivilProtectionApprovalBackUrl,
                    e.EventInsuranceUrl,
                    e.StripePaymentLink
                })
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(events);
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

        // AdminController.cs

        [HttpGet("place-availability/{placeId}")]
        public async Task<IActionResult> GetPlaceAvailability(int placeId)
        {
            // Get ALL blocked and available dates for this place
            var availabilities = await _context.PlaceAvailabilities
                .Where(a => a.PlaceId == placeId)
                .Select(a => new {
                    a.Id,
                    a.Date,
                    a.IsBlocked,
                    a.Note
                })
                .ToListAsync();

            return Ok(availabilities);
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

        [HttpPost("reject-place/{id}")]
        public async Task<IActionResult> RejectPlace(int id, [FromBody] PlaceRejectionDto dto)
        {
            var place = await _context.Places.FindAsync(id);
            if (place == null)
                return NotFound(new { message = "Place not found." });

            place.IsApproved = false;
            place.AdminNote = dto.AdminNote; // Save the rejection reason!
            await _context.SaveChangesAsync();

            return Ok(new { message = "Place rejected." });
        }


        [HttpGet("pending-provider-requests")]
        public async Task<IActionResult> GetPendingProviderRequests()
        {
            var requests = await _context.ServiceProviderRequests
                .Where(r => r.IsApproved == null)
                .Include(r => r.User)
                .Select(r => new ServiceProviderRequestDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.UserName,
                    Email = r.User.Email,
                    NationalIdFrontUrl = r.NationalIdFrontUrl,
                    NationalIdBackUrl = r.NationalIdBackUrl,
                    HoldingIdUrl = r.HoldingIdUrl,
                    StripePaymentLink = r.StripePaymentLink,
                    RequestedAt = r.RequestedAt,
                    IsApproved = r.IsApproved,
                    AdminNote = r.AdminNote
                }).ToListAsync();
            return Ok(requests);
        }

        [HttpPost("approve-provider-request/{id}")]
        public async Task<IActionResult> ApproveProviderRequest(int id)
        {
            var req = await _context.ServiceProviderRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return NotFound();
            if (req.IsApproved == true) return BadRequest(new { message = "Already approved." });

            req.IsApproved = true;
            req.AdminNote = null;

            // Promote user to Service Provider
            await _userManager.AddToRoleAsync(req.User, "Service Provider");
            await _userManager.RemoveFromRoleAsync(req.User, "User");
            await _context.SaveChangesAsync();
            return Ok(new { message = "Request approved and user promoted." });
        }

        [HttpPost("reject-provider-request/{id}")]
        public async Task<IActionResult> RejectProviderRequest(int id, [FromBody] string adminNote)
        {
            var req = await _context.ServiceProviderRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return NotFound();
            if (req.IsApproved == false) return BadRequest(new { message = "Already rejected." });

            req.IsApproved = false;
            req.AdminNote = adminNote;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Request rejected." });
        }

        // 1. Get all users (with search & filter)
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? name = null, [FromQuery] string? email = null, [FromQuery] string? role = null)
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    Roles = (from userRole in _context.UserRoles
                             join r in _context.Roles on userRole.RoleId equals r.Id
                             where userRole.UserId == u.Id
                             select r.Name).ToList()
                })
                .ToListAsync();

            // Apply search filters if provided
            if (!string.IsNullOrEmpty(name))
                users = users.Where(u => u.UserName.ToLower().Contains(name.ToLower())).ToList();
            if (!string.IsNullOrEmpty(email))
                users = users.Where(u => (u.Email ?? "").ToLower().Contains(email.ToLower())).ToList();
            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Roles.Any(r => r.ToLower() == role.ToLower())).ToList();

            return Ok(users);
        }

        // 2. Get user details
        [HttpGet("user-details/{id}")]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.BirthDate,
                user.LastName,
                user.Gender,
                user.CreatedAt,
                Roles = roles
            });
        }

        // 3. Make Admin
        [HttpPost("make-admin-by-username/{userName}")]
        public async Task<IActionResult> MakeAdmin(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return NotFound();

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "User promoted to Admin." });
        }

        // 4. Remove Admin
        [HttpPost("remove-admin-by-username/{userName}")]
        public async Task<IActionResult> RemoveAdmin(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            // Ensure he has at least "User" role
            if (!await _userManager.IsInRoleAsync(user, "User"))
                await _userManager.AddToRoleAsync(user, "User");

            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "Admin role removed. User is now a normal user." });
        }

        // 5. Remove Service Provider role
        [HttpPost("remove-service-provider/{userName}")]
        public async Task<IActionResult> RemoveServiceProvider(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, "Service Provider");
            // Ensure he has at least "User" role
            if (!await _userManager.IsInRoleAsync(user, "User"))
                await _userManager.AddToRoleAsync(user, "User");

            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "Service Provider role removed. User is now a normal user." });
        }

        // 6. Delete User
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "User deleted successfully." });
        }

    }
}
