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
    public class CarsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CarsController(ApplicationDbContext db) => _db = db;

        // ---------- READ (browse inventory) ----------
        /// <summary>List cars (optional filters: year, make). Public.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> Get([FromQuery] int? year, [FromQuery] string? make)
        {
            var q = _db.Cars.AsQueryable();
            if (year.HasValue) q = q.Where(c => c.Year == year.Value);
            if (!string.IsNullOrWhiteSpace(make)) q = q.Where(c => c.Make == make);
            return Ok(await q.ToListAsync());
        }

        /// <summary>Get a single car by id. Public.</summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Car>> GetOne(Guid id)
            => await _db.Cars.FindAsync(id) is Car c ? Ok(c) : NotFound();

        // ---------- CREATE (Admin) ----------
        /// <summary>Create a new car. Admin only.</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Car>> Create(Car car)
        {
            _db.Cars.Add(car);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = car.Id }, car);
        }

        // ---------- UPDATE (Admin + OTP) ----------
        /// <summary>Update a car. Admin + OTP required (X-OTP-Purpose & X-OTP-Code).</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            Car input,
            [FromServices] IOtpService otp,
            [FromHeader(Name = "X-OTP-Purpose")] string purpose,
            [FromHeader(Name = "X-OTP-Code")] string code)
        {
            // Expected purpose: "updateVehicle:<carId>"
            if (!otp.Validate(purpose, code)) return BadRequest("OTP invalid or expired");
            if (id != input.Id) return BadRequest("Id mismatch");

            var car = await _db.Cars.FindAsync(id);
            if (car is null) return NotFound();

            car.Make = input.Make;
            car.Model = input.Model;
            car.Year = input.Year;
            car.Price = input.Price;
            car.IsAvailable = input.IsAvailable;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- DELETE (Admin) ----------
        /// <summary>Delete a car. Admin only.</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var car = await _db.Cars.FindAsync(id);
            if (car is null) return NotFound();

            _db.Cars.Remove(car);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
