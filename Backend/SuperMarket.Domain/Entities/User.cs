using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class User : Entity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.Cashier;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = [];
}

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Cashier = 3,
    StockKeeper = 4
}
