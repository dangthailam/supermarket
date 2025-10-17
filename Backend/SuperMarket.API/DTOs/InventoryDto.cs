using SuperMarket.API.Models;

namespace SuperMarket.API.DTOs;

public class InventoryMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInventoryMovementDto
{
    public int ProductId { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class StockAdjustmentDto
{
    public int ProductId { get; set; }
    public int NewQuantity { get; set; }
    public string? Reason { get; set; }
}
