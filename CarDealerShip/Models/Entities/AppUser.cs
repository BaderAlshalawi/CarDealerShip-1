namespace CarDealerShip.Models.Entities
{
    public class AppUser
    {
        public required string Id { get; set; }      // Unique user ID
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string Role { get; set; }    // "Admin" or "Customer"
    }
}
