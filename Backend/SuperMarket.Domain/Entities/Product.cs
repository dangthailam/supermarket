using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class Product : Entity
{
    public string SKU { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal CostPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public int MinStockLevel { get; private set; } = 10;
    public int? MaxStockLevel { get; private set; }
    public Guid CategoryId { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    // New fields from Excel import
    public string? ProductType { get; private set; }  // Loại hàng: Hàng hóa, dịch vụ hay combo
    public Guid? BrandId { get; private set; }  // Thương hiệu
    public Brand? Brand { get; set; }
    public string? Unit { get; private set; }  // ĐVT (Unit of Measure)
    public decimal? Weight { get; private set; }  // Trọng lượng
    public string? Location { get; private set; }  // Vị trí
    public bool DirectSalesEnabled { get; private set; } = true;  // Được bán trực tiếp
    public bool PointsEnabled { get; private set; } = false;  // Tích điểm

    // Navigation properties
    public Category Category { get; private set; } = null!;
    public ICollection<ProductBarcode> Barcodes { get; private set; } = [];
    public ICollection<TransactionItem> TransactionItems { get; private set; } = [];
    public ICollection<InventoryMovement> InventoryMovements { get; private set; } = [];
    public ICollection<PurchaseItem> PurchaseItems { get; private set; } = [];

    // Parameterless constructor for EF Core
    private Product()
    {
    }

    // Methods to update product details
    public void UpdateStock(int quantity)
    {
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be null or empty", nameof(sku));
        
        SKU = sku;
        UpdatedAt = DateTime.UtcNow;
    }

    public ProductBarcode? AddBarcode(string barcode, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new ArgumentException("Barcode cannot be null or empty", nameof(barcode));

        var normalizedBarcode = barcode.Trim();
        if (Barcodes.Any(b => b.Barcode == normalizedBarcode))
            return null; // Barcode already exists

        if (isPrimary)
        {
            // Unset existing primary barcodes
            foreach (var existingBarcode in Barcodes.Where(b => b.IsPrimary))
            {
                existingBarcode.UnsetPrimary();
            }
        }

        var productBarcode = new ProductBarcode(Id, normalizedBarcode, isPrimary);
        Barcodes.Add(productBarcode);
        UpdatedAt = DateTime.UtcNow;

        return productBarcode;
    }

    public ProductBarcode? RemoveBarcode(string barcode)
    {
        var barcodeToRemove = Barcodes.FirstOrDefault(b => b.Barcode == barcode.Trim());
        if (barcodeToRemove != null)
        {
            Barcodes.Remove(barcodeToRemove);
            UpdatedAt = DateTime.UtcNow;
        }
        return barcodeToRemove;
    }

    public string? GetPrimaryBarcode()
    {
        return Barcodes.FirstOrDefault(b => b.IsPrimary)?.Barcode ?? Barcodes.FirstOrDefault()?.Barcode;
    }

    public Product(string name, Category category, decimal price, decimal costPrice)
    {
        Name = name;
        Category = category;
        CategoryId = category.Id;
        Price = price;
        CostPrice = costPrice;
    }

    public void UpdateDetails(string name, string? description, decimal price, decimal costPrice, int minStockLevel, int? maxStockLevel, bool isActive,
                              string? productType, Brand? brand, string? unit, decimal? weight, string? location,
                              bool directSalesEnabled, bool pointsEnabled, string? imageUrl)
    {
        Name = name;
        Description = description;
        Price = price;
        CostPrice = costPrice;
        MinStockLevel = minStockLevel;
        MaxStockLevel = maxStockLevel;
        IsActive = isActive;
        ProductType = productType;
        Brand = brand;
        BrandId = brand?.Id;
        Unit = unit;
        Weight = weight;
        Location = location;
        DirectSalesEnabled = directSalesEnabled;
        PointsEnabled = pointsEnabled;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    
}
