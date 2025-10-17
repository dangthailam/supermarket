namespace SuperMarket.API.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public int? MaxStockLevel { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }

    // New fields from Excel import
    public string? ProductType { get; set; }
    public string? Brand { get; set; }
    public string? Unit { get; set; }
    public decimal? Weight { get; set; }
    public string? Location { get; set; }
    public bool DirectSalesEnabled { get; set; }
    public bool PointsEnabled { get; set; }

    public bool LowStock => StockQuantity <= MinStockLevel;
}

public class CreateProductDto
{
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

    // New fields from Excel import
    public string? ProductType { get; set; }
    public string? Brand { get; set; }
    public string? Unit { get; set; }
    public decimal? Weight { get; set; }
    public string? Location { get; set; }
    public bool DirectSalesEnabled { get; set; } = true;
    public bool PointsEnabled { get; set; } = false;
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public decimal? Price { get; set; }
    public decimal? CostPrice { get; set; }
    public int? MinStockLevel { get; set; }
    public int? MaxStockLevel { get; set; }
    public int? CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }

    // New fields from Excel import
    public string? ProductType { get; set; }
    public string? Brand { get; set; }
    public string? Unit { get; set; }
    public decimal? Weight { get; set; }
    public string? Location { get; set; }
    public bool? DirectSalesEnabled { get; set; }
    public bool? PointsEnabled { get; set; }
}
