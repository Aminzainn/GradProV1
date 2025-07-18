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
    public class PlaceController : ControllerBase
    {
        private readonly EventManagerContext _context;

        public PlaceController(EventManagerContext context)
        {
            _context = context;
        }

        // ✅ Get only places added by the current Service Provider
        [HttpGet("my-places")]
        [Authorize(Roles = "Service Provider")]
        public async Task<IActionResult> GetMyPlaces()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var places = await _context.Places
                .Include(p => p.PlaceType)
                .Where(p => p.CreatedByUserId == userId)
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

        // ✅ Add Place (linked to Service Provider)
        [HttpPost("add")]
        [Authorize(Roles = "Service Provider")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPlace([FromForm] AddPlaceDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? imagePath = null;

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
                IsApproved = false,
                ImageUrl = imagePath,
                CreatedByUserId = userId
            };

            _context.Places.Add(place);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Place added successfully and is waiting for admin approval." });
        }
    }
}
