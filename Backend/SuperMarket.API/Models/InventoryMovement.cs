namespace SuperMarket.API.Models;

public class InventoryMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public User? User { get; set; }
}

public enum MovementType
{
    Purchase = 1,      // Stock in
    Sale = 2,          // Stock out
    Adjustment = 3,    // Manual adjustment
    Return = 4,        // Return to supplier
    Damage = 5,        // Damaged goods
    Transfer = 6       // Transfer between locations
}
