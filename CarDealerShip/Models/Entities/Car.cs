namespace CarDealerShip.Models.Entities
{
    public class Car
    {
        public Guid Id { get; set; }          // Primary key
        public required string Make { get; set; }     // e.g., Toyota
        public required string Model { get; set; }    // e.g., Corolla
        public int Year { get; set; }        // e.g., 2022
        public decimal Price { get; set; }   // e.g., 20000.50
        public bool IsAvailable { get; set; } = true; // Is the car available for sale?
    }
}
