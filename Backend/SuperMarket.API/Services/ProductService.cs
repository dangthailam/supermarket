using Microsoft.EntityFrameworkCore;
using SuperMarket.API.Data;
using SuperMarket.API.DTOs;
using SuperMarket.API.Interfaces;
using SuperMarket.API.Models;

namespace SuperMarket.API.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SuperMarketDbContext _context;

    public ProductService(IUnitOfWork unitOfWork, SuperMarketDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .AsNoTracking()
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= p.MinStockLevel && p.IsActive)
            .AsNoTracking()
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto?> GetProductByBarcodeAsync(string barcode)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Barcode == barcode);

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            SKU = dto.SKU,
            Name = dto.Name,
            Description = dto.Description,
            Barcode = dto.Barcode,
            Price = dto.Price,
            CostPrice = dto.CostPrice,
            StockQuantity = dto.StockQuantity,
            MinStockLevel = dto.MinStockLevel,
            MaxStockLevel = dto.MaxStockLevel,
            CategoryId = dto.CategoryId,
            ImageUrl = dto.ImageUrl,
            ProductType = dto.ProductType,
            Brand = dto.Brand,
            Unit = dto.Unit,
            Weight = dto.Weight,
            Location = dto.Location,
            DirectSalesEnabled = dto.DirectSalesEnabled,
            PointsEnabled = dto.PointsEnabled
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var createdProduct = await _context.Products
            .Include(p => p.Category)
            .FirstAsync(p => p.Id == product.Id);

        return MapToDto(createdProduct);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return null;

        if (!string.IsNullOrEmpty(dto.Name))
            product.Name = dto.Name;
        if (dto.Description != null)
            product.Description = dto.Description;
        if (dto.Barcode != null)
            product.Barcode = dto.Barcode;
        if (dto.Price.HasValue)
            product.Price = dto.Price.Value;
        if (dto.CostPrice.HasValue)
            product.CostPrice = dto.CostPrice.Value;
        if (dto.MinStockLevel.HasValue)
            product.MinStockLevel = dto.MinStockLevel.Value;
        if (dto.MaxStockLevel.HasValue)
            product.MaxStockLevel = dto.MaxStockLevel.Value;
        if (dto.CategoryId.HasValue)
            product.CategoryId = dto.CategoryId.Value;
        if (dto.ImageUrl != null)
            product.ImageUrl = dto.ImageUrl;
        if (dto.IsActive.HasValue)
            product.IsActive = dto.IsActive.Value;
        if (dto.ProductType != null)
            product.ProductType = dto.ProductType;
        if (dto.Brand != null)
            product.Brand = dto.Brand;
        if (dto.Unit != null)
            product.Unit = dto.Unit;
        if (dto.Weight.HasValue)
            product.Weight = dto.Weight.Value;
        if (dto.Location != null)
            product.Location = dto.Location;
        if (dto.DirectSalesEnabled.HasValue)
            product.DirectSalesEnabled = dto.DirectSalesEnabled.Value;
        if (dto.PointsEnabled.HasValue)
            product.PointsEnabled = dto.PointsEnabled.Value;

        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        var updatedProduct = await _context.Products
            .Include(p => p.Category)
            .FirstAsync(p => p.Id == id);

        return MapToDto(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && (
                p.Name.Contains(searchTerm) ||
                p.SKU.Contains(searchTerm) ||
                (p.Barcode != null && p.Barcode.Contains(searchTerm))
            ))
            .AsNoTracking()
            .ToListAsync();

        return products.Select(MapToDto);
    }

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Barcode = product.Barcode,
            Price = product.Price,
            CostPrice = product.CostPrice,
            StockQuantity = product.StockQuantity,
            MinStockLevel = product.MinStockLevel,
            MaxStockLevel = product.MaxStockLevel,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            ProductType = product.ProductType,
            Brand = product.Brand,
            Unit = product.Unit,
            Weight = product.Weight,
            Location = product.Location,
            DirectSalesEnabled = product.DirectSalesEnabled,
            PointsEnabled = product.PointsEnabled
        };
    }
}
