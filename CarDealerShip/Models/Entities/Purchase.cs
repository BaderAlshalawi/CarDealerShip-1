using CarDealerShip.Models.Entities;

public class Purchase
{
    public Guid Id { get; set; } = Guid.NewGuid();   // PK
    public required string CustomerId { get; set; }  // FK -> AppUser.Id
    public Guid CarId { get; set; }                  // FK -> Car.Id
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    // Navigation properties (optional but useful with EF Core)
    public Car? Car { get; set; }
    public AppUser? Customer { get; set; }
}
