using System.Security.Claims;
using CarDealerShip.Data;
using CarDealerShip.Models.Entities;
using CarDealerShip.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealerShip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public PurchasesController(ApplicationDbContext db) => _db = db;

        // ========================= READ (Admin) =========================

        /// <summary>
        /// List all purchases (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Purchase>>> GetAll()
        {
            var list = await _db.Purchases
                .Include(p => p.Car)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Get a single purchase by id (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Purchase>> GetOne(Guid id)
        {
            var p = await _db.Purchases
                .Include(x => x.Car)
                .FirstOrDefaultAsync(x => x.Id == id);

            return p is null ? NotFound() : Ok(p);
        }

        /// <summary>
        /// Get purchases for a specific customer (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("by-customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<Purchase>>> ByCustomer(string customerId)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == customerId))
                return NotFound("Customer not found");

            var list = await _db.Purchases
                .Where(p => p.CustomerId == customerId)
                .Include(p => p.Car)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return Ok(list);
        }

        // ========================= READ (Customer) ======================

        /// <summary>
        /// Get the authenticated customer's purchases (Customer only).
        /// </summary>
        [Authorize(Roles = "Customer")]
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Purchase>>> MyPurchases()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var list = await _db.Purchases
                .Where(p => p.CustomerId == userId)
                .Include(p => p.Car)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return Ok(list);
        }

        // ========================= CREATE (Customer + OTP) ==============

        public class CreatePurchaseDto
        {
            public required Guid CarId { get; set; }
        }

        /// <summary>
        /// Create a purchase (Customer + OTP).  
        /// Send header <c>X-OTP-Code</c> with the code generated for purpose <c>purchase:&lt;userId&gt;:&lt;carId&gt;</c>.
        /// </summary>
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<ActionResult<Purchase>> Create(
            [FromBody] CreatePurchaseDto dto,                    // <-- single body param
            [FromServices] IOtpService otp,                      // injected service
            [FromHeader(Name = "X-OTP-Code")] string otpCode)    // OTP comes from header
        {
            // user id comes from JWT; do NOT bind ClaimsPrincipal from body
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // validate OTP: must exactly match the purpose used at /api/AppUsers/otp/start
            var purpose = $"purchase:{userId}:{dto.CarId}";
            if (!otp.Validate(purpose, otpCode))
                return BadRequest("OTP invalid or expired");

            // validate user
            if (!await _db.Users.AnyAsync(u => u.Id == userId))
                return BadRequest("Customer does not exist");

            // validate car
            var car = await _db.Cars.FindAsync(dto.CarId);
            if (car is null)
                return BadRequest("Car does not exist");
            if (!car.IsAvailable)
                return BadRequest("Car is not available");

            // commit purchase (car becomes unavailable)
            car.IsAvailable = false;

            var purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                CustomerId = userId,
                CarId = dto.CarId
                // PurchaseDate uses DB default (GETUTCDATE or CURRENT_TIMESTAMP)
            };

            _db.Purchases.Add(purchase);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = purchase.Id }, purchase);
        }

        // (No UPDATE/DELETE for purchases in this simple flow;
        //  if you need them, add Admin-only endpoints with appropriate business rules.)
    }
}
