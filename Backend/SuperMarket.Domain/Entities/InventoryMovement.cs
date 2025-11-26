using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class InventoryMovement : Entity
{
    public Guid ProductId { get; private set; }
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public Guid? UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public User? User { get; private set; }

    public InventoryMovement(Guid productId, MovementType type, int quantity, string? reference = null, string? notes = null)
    {
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        Reference = reference;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }
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
