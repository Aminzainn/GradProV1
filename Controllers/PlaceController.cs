using GP.Models;
using GP.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly EventManagerContext _context;
        private readonly IWebHostEnvironment _env;

        public PlaceController(EventManagerContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Get places added by the current Service Provider
        [HttpGet("my-places")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> GetMyPlaces()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var places = await _context.Places
                .Where(p => p.CreatedByUserId == userId)
                .Select(p => new MyPlaceDto
                {
                    Id = p.Id,
                    Location = p.Location,
                    MaxAttendees = p.MaxAttendees,
                    PlaceTypeName = p.PlaceTypeName, // Get the PlaceTypeName directly
                    IsApproved = p.IsApproved,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    SecurityClearanceUrl = p.SecurityClearanceUrl,
                    OwnershipOrRentalContractUrl = p.OwnershipOrRentalContractUrl,
                    NationalIdFrontUrl = p.NationalIdFrontUrl,
                    NationalIdBackUrl = p.NationalIdBackUrl,
                    StripePaymentLink = p.StripePaymentLink,
                    Latitude = p.Latitude, // Location latitude
                    Longitude = p.Longitude // Location longitude
                })
                .ToListAsync();

            return Ok(places);
        }

        // Add a new Place (linked to Service Provider)
        [HttpPost("add")]
        [Authorize(Roles = "Service Provider")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPlace([FromForm] AddPlaceDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "places");
            Directory.CreateDirectory(uploadsFolder);

            // Saving images
            string? imageUrl = await SaveFileAsync(model.Image, uploadsFolder, "image");
            string? securityClearanceUrl = await SaveFileAsync(model.SecurityClearance, uploadsFolder, "security");
            string? ownershipOrRentalContractUrl = await SaveFileAsync(model.OwnershipOrRentalContract, uploadsFolder, "contract");
            string? nationalIdFrontUrl = await SaveFileAsync(model.NationalIdFront, uploadsFolder, "id_front");
            string? nationalIdBackUrl = await SaveFileAsync(model.NationalIdBack, uploadsFolder, "id_back");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Create a new place and save the location with latitude and longitude
            var place = new Place
            {
                Location = model.Location,
                MaxAttendees = model.MaxAttendees,
                PlaceTypeName = model.PlaceTypeName, // Use PlaceTypeName directly
                Price = model.Price,
                IsApproved = false,
                ImageUrl = imageUrl,
                SecurityClearanceUrl = securityClearanceUrl,
                OwnershipOrRentalContractUrl = ownershipOrRentalContractUrl,
                NationalIdFrontUrl = nationalIdFrontUrl,
                NationalIdBackUrl = nationalIdBackUrl,
                StripePaymentLink = model.StripePaymentLink,
                CreatedByUserId = userId,
                Latitude = model.Latitude, // Store latitude from the frontend
                Longitude = model.Longitude // Store longitude from the frontend
            };

            try
            {
                _context.Places.Add(place);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Place added successfully and is waiting for admin approval." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // Helper method for file saving
        private async Task<string?> SaveFileAsync(IFormFile? file, string folder, string label)
        {
            if (file == null) return null;
            string fileName = $"{Guid.NewGuid()}_{label}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(folder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/images/places/{fileName}";
        }

        // Get details of a single place
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPlaceDetails(int id)
        {
            var place = await _context.Places
                .Where(p => p.Id == id)
                .Select(p => new MyPlaceDto
                {
                    Id = p.Id,
                    Location = p.Location,
                    MaxAttendees = p.MaxAttendees,
                    PlaceTypeName = p.PlaceTypeName, // Use the PlaceTypeName directly
                    IsApproved = p.IsApproved,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    SecurityClearanceUrl = p.SecurityClearanceUrl,
                    OwnershipOrRentalContractUrl = p.OwnershipOrRentalContractUrl,
                    NationalIdFrontUrl = p.NationalIdFrontUrl,
                    NationalIdBackUrl = p.NationalIdBackUrl,
                    StripePaymentLink = p.StripePaymentLink,
                    Latitude = p.Latitude, // Location latitude
                    Longitude = p.Longitude // Location longitude
                })
                .FirstOrDefaultAsync();

            if (place == null) return NotFound();
            return Ok(place);
        }

        // Edit a place (update details of an existing place)
        [HttpPut("edit/{id}")]
        [Authorize(Roles = "Service Provider")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EditPlace(int id, [FromForm] AddPlaceDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == id && p.CreatedByUserId == userId);
            if (place == null) return NotFound();

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "places");
            Directory.CreateDirectory(uploadsFolder);

            place.Location = model.Location;
            place.MaxAttendees = model.MaxAttendees;
            place.PlaceTypeName = model.PlaceTypeName; // Update PlaceTypeName directly
            place.Price = model.Price;
            place.StripePaymentLink = model.StripePaymentLink;

            if (model.Image != null)
                place.ImageUrl = await SaveFileAsync(model.Image, uploadsFolder, "image");
            if (model.SecurityClearance != null)
                place.SecurityClearanceUrl = await SaveFileAsync(model.SecurityClearance, uploadsFolder, "security");
            if (model.OwnershipOrRentalContract != null)
                place.OwnershipOrRentalContractUrl = await SaveFileAsync(model.OwnershipOrRentalContract, uploadsFolder, "contract");
            if (model.NationalIdFront != null)
                place.NationalIdFrontUrl = await SaveFileAsync(model.NationalIdFront, uploadsFolder, "id_front");
            if (model.NationalIdBack != null)
                place.NationalIdBackUrl = await SaveFileAsync(model.NationalIdBack, uploadsFolder, "id_back");

            // Revert approval after edit
            place.IsApproved = false;
            place.Latitude = model.Latitude; // Update latitude from frontend
            place.Longitude = model.Longitude; // Update longitude from frontend

            await _context.SaveChangesAsync();

            return Ok(new { message = "Place updated. Waiting for admin approval." });
        }

        // Soft delete a place (actually deletes it in this case)
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> DeletePlace(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == id && p.CreatedByUserId == userId);
            if (place == null) return NotFound();

            _context.Places.Remove(place);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Place deleted." });
        }

        // GET: api/Place/{placeId}/availability
        [HttpGet("{placeId}/availability")]
        [Authorize(Roles = "Service Provider, Admin")]
        public async Task<IActionResult> GetPlaceAvailability(int placeId)
        {
            var availability = await _context.PlaceAvailabilities
                .Where(a => a.PlaceId == placeId)
                .Select(a => new {
                    id = a.Id,
                    date = a.Date,
                    isBlocked = a.IsBlocked
                })
                .ToListAsync();

            return Ok(availability);
        }

        // POST: api/Place/{placeId}/availability/block
        [HttpPost("{placeId}/availability/block")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> BlockAvailability(int placeId, [FromBody] List<DateTime> dates)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == placeId && p.CreatedByUserId == userId);
            if (place == null) return NotFound();

            foreach (var date in dates)
            {
                if (!_context.PlaceAvailabilities.Any(a => a.PlaceId == placeId && a.Date == date.Date))
                {
                    _context.PlaceAvailabilities.Add(new PlaceAvailability
                    {
                        PlaceId = placeId,
                        Date = date.Date,
                        IsBlocked = true
                    });
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Blocked dates added." });
        }

        // DELETE: api/Place/availability/{availabilityId}
        [HttpDelete("availability/{availabilityId}")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> UnblockAvailability(int availabilityId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var availability = await _context.PlaceAvailabilities
                .Include(a => a.Place)
                .FirstOrDefaultAsync(a => a.Id == availabilityId && a.Place.CreatedByUserId == userId);
            if (availability == null) return NotFound();

            _context.PlaceAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Date unblocked." });
        }
    }
}
