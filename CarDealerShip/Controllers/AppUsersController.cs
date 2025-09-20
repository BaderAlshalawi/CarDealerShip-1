using CarDealerShip.Data;
using CarDealerShip.Models.Entities;
using CarDealerShip.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CarDealerShip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _cfg;
        private readonly IOtpService _otp;

        public AppUsersController(ApplicationDbContext db, IConfiguration cfg, IOtpService otp)
        {
            _db = db;
            _cfg = cfg;
            _otp = otp;
        }

        // ---------- Helper to create JWT ----------
        private string CreateJwt(AppUser user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ---------- OTP ----------
        /// <summary>
        /// Generate an OTP code for a specific purpose (e.g., register, login, update, purchase).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("otp/start")]
        public ActionResult StartOtp([FromBody] OtpStartDto dto)
        {
            _otp.Generate(dto.Purpose, TimeSpan.FromMinutes(3));
            return Ok(new { message = "OTP generated. Check server logs.", expiresInSeconds = 180 });
        }
        public class OtpStartDto { public required string Purpose { get; set; } }

        // ---------- READ ----------
        /// <summary>
        /// Get all users (optionally filter by role).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetAll([FromQuery] string? role)
        {
            var q = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(role)) q = q.Where(u => u.Role == role);
            return Ok(await q.ToListAsync());
        }

        /// <summary>
        /// Get a single user by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetOne(string id)
        {
            var user = await _db.Users.FindAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        // ---------- CREATE ----------
        public class RegisterWithOtpDto
        {
            public required string Email { get; set; }
            public required string Password { get; set; }   // demo only; replace with Identity later
            public required string Otp { get; set; }
            public string Role { get; set; } = "Customer";
        }

        /// <summary>
        /// Register a new user (requires OTP).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register(RegisterWithOtpDto dto)
        {
            if (!_otp.Validate($"register:{dto.Email}", dto.Otp))
                return BadRequest("OTP invalid or expired");

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already registered");

            var user = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = dto.Email,
                PasswordHash = dto.Password, // ⚠️ only for demo
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "Customer" : dto.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = CreateJwt(user);
            return CreatedAtAction(nameof(GetOne), new { id = user.Id }, new { token, user.Id, user.Email, user.Role });
        }

        public class LoginWithOtpDto
        {
            public required string Email { get; set; }
            public required string Password { get; set; }
            public required string Otp { get; set; }
        }

        /// <summary>
        /// Login an existing user (requires OTP).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginWithOtpDto dto)
        {
            if (!_otp.Validate($"login:{dto.Email}", dto.Otp))
                return BadRequest("OTP invalid or expired");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user is null || user.PasswordHash != dto.Password)
                return Unauthorized("Invalid credentials");

            var token = CreateJwt(user);
            return Ok(new { token, user.Id, user.Email, user.Role });
        }

        // ---------- UPDATE ----------
        /// <summary>
        /// Update a user (role or password).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] AppUser update)
        {
            if (id != update.Id) return BadRequest();
            var exists = await _db.Users.AnyAsync(u => u.Id == id);
            if (!exists) return NotFound();

            _db.Entry(update).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- DELETE ----------
        /// <summary>
        /// Delete a user by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
