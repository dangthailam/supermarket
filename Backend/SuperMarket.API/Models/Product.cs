namespace SuperMarket.API.Models;

public class Product
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; } = 10;
    public int? MaxStockLevel { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // New fields from Excel import
    public string? ProductType { get; set; }  // Loại hàng
    public string? Brand { get; set; }  // Thương hiệu
    public string? Unit { get; set; }  // ĐVT (Unit of Measure)
    public decimal? Weight { get; set; }  // Trọng lượng
    public string? Location { get; set; }  // Vị trí
    public bool DirectSalesEnabled { get; set; } = true;  // Được bán trực tiếp
    public bool PointsEnabled { get; set; } = false;  // Tích điểm

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
