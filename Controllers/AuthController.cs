using GP.Models;
using GP.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting; // <== add this
using Microsoft.EntityFrameworkCore;
using GP.Models.DTO; // <== add this


namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly EventManagerContext _context; // <== add this
        private readonly IWebHostEnvironment _env; // <== add this


        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> jwtSettings,
            EventManagerContext context, // <== add this
            IWebHostEnvironment env // <== add this


            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _context = context; // <== add this
            _env = env;         // <== add this

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _userManager.FindByNameAsync(model.UserName) != null)
                return BadRequest(new { message =  "Username is already taken" });

            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return BadRequest(new { message = "Email is already registered" });

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                LastName = model.LastName,
                Gender = model.Gender,
                BirthDate = model.BirthDate,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.UserNameOrEmail)
                    ?? await _userManager.FindByEmailAsync(model.UserNameOrEmail);

            if (user == null)
                return Unauthorized(new { message = "Invalid Email or Username" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid Password" });

            var token = await GenerateJwtToken(user);

            return Ok(new
            {
                token,
                userName = user.UserName,
                email = user.Email,
                roles = await _userManager.GetRolesAsync(user)
            });
        }

        // POST: api/Auth/request-provider
        [Authorize]
        [HttpPost("request-service-provider")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RequestServiceProvider([FromForm] SubmitServiceProviderRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user already has a pending request
            if (_context.ServiceProviderRequests.Any(r => r.UserId == userId && r.IsApproved == null))
                return BadRequest(new { message = "You already have a pending request." });

            // Save files
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "provider-requests");
            Directory.CreateDirectory(uploadsFolder);

            string SaveFile(IFormFile file, string label)
            {
                var fileName = $"{Guid.NewGuid()}_{label}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(stream);
                return $"/images/provider-requests/{fileName}";
            }

            var req = new ServiceProviderRequest
            {
                UserId = userId,
                NationalIdFrontUrl = SaveFile(dto.NationalIdFront, "id_front"),
                NationalIdBackUrl = SaveFile(dto.NationalIdBack, "id_back"),
                HoldingIdUrl = SaveFile(dto.HoldingId, "holding_id"),
                StripePaymentLink = dto.StripePaymentLink,
                RequestedAt = DateTime.UtcNow
            };
            _context.ServiceProviderRequests.Add(req);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request submitted. Await admin approval." });
        }


        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("id", user.Id), // ✅ أضف السطر ده,
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
