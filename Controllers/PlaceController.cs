using GP.Models;
using GP.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Service Provider")]
    public class PlaceController : ControllerBase
    {
        private readonly EventManagerContext _context;

        public PlaceController(EventManagerContext context)
        {
            _context = context;
        }

        // ✅ Get places added by the Service Provider
        [HttpGet("my-places")]
        public async Task<IActionResult> GetMyPlaces()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var places = await _context.Places
                .Where(p => p.CreatedByUserId == userId && !p.IsDeleted)
                .Select(p => new MyPlaceDto
                {
                    Id = p.Id,
                    Location = p.Location,
                    MaxAttendees = p.MaxAttendees,
                    PlaceTypeName = p.PlaceType.Name,
                    IsApproved = p.IsApproved,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Ok(places);
        }

        // ✅ Add a new place
        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPlace([FromForm] AddPlaceDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? imagePath = null;
            // Handle image upload
            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "places");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imagePath = $"/images/places/{uniqueFileName}";
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var place = new Place
            {
                Location = model.Location,
                MaxAttendees = model.MaxAttendees,
                PlaceTypeId = model.PlaceTypeId,
                IsApproved = false,  // Initially set to false, pending admin approval.
                ImageUrl = imagePath,
                CreatedByUserId = userId,
                SecurityClearanceUrl = model.SecurityClearance,
                OwnershipContractUrl = model.OwnershipContract,
                NationalIdFrontUrl = model.NationalIdFront,
                NationalIdBackUrl = model.NationalIdBack,
                StripePaymentLink = model.StripePaymentLink
            };

            _context.Places.Add(place);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Place added successfully and is waiting for admin approval." });
        }

        // ✅ Edit place details
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditPlace(int id, [FromForm] AddPlaceDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var place = await _context.Places
                .FirstOrDefaultAsync(p => p.Id == id && p.CreatedByUserId == userId && !p.IsDeleted);

            if (place == null) return NotFound("Place not found.");

            // Update place info
            place.Location = model.Location ?? place.Location;
            place.MaxAttendees = model.MaxAttendees != 0 ? model.MaxAttendees : place.MaxAttendees;

            // Update Image (if uploaded)
            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "places");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                place.ImageUrl = $"/images/places/{uniqueFileName}";
            }

            // Set place back to pending approval after edit
            place.IsApproved = false;

            // Add documents if they are uploaded
            place.SecurityClearanceUrl = model.SecurityClearance ?? place.SecurityClearanceUrl;
            place.OwnershipContractUrl = model.OwnershipContract ?? place.OwnershipContractUrl;
            place.NationalIdFrontUrl = model.NationalIdFront ?? place.NationalIdFrontUrl;
            place.NationalIdBackUrl = model.NationalIdBack ?? place.NationalIdBackUrl;
            place.StripePaymentLink = model.StripePaymentLink ?? place.StripePaymentLink;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Place updated successfully. Awaiting admin approval." });
        }

        // ✅ Delete place (soft delete)
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePlace(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var place = await _context.Places
                .FirstOrDefaultAsync(p => p.Id == id && p.CreatedByUserId == userId && !p.IsDeleted);

            if (place == null) return NotFound("Place not found.");

            place.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Place deleted successfully." });
        }
    }
}
