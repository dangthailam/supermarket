using SuperMarket.Application.DTOs;
using SuperMarket.Application.Interfaces;
using SuperMarket.Domain.Common;
using SuperMarket.Domain.Entities;

namespace SuperMarket.Application.Services;

public interface IProductService
{
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<bool> DeleteProductAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
    Task<ProductDto?> GetProductByBarcodeAsync(string barcode);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
    Task<PaginatedResult<ProductDto>> GetProductsPagedAsync(PaginationParams paginationParams);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto dto);
}

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return await MapToDtosAsync(products);
    }

    public async Task<PaginatedResult<ProductDto>> GetProductsPagedAsync(PaginationParams paginationParams)
    {
        // Build filter expression
        Func<IQueryable<Product>, IQueryable<Product>> filter = query =>
        {
            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var searchTerm = paginationParams.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(searchTerm)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(searchTerm))
                );
            }
            return query;
        };

        // Build sort expression
        Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = query =>
        {
            return paginationParams.SortBy?.ToLower() switch
            {
                "name" => paginationParams.SortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "sku" => paginationParams.SortDescending
                    ? query.OrderByDescending(p => p.SKU)
                    : query.OrderBy(p => p.SKU),
                "price" => paginationParams.SortDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "stock" => paginationParams.SortDescending
                    ? query.OrderByDescending(p => p.StockQuantity)
                    : query.OrderBy(p => p.StockQuantity),
                "createdat" => paginationParams.SortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Id)
            };
        };

        // Use repository's GetPagedAsync but we need to handle the filter differently
        // For now, let's get all matching items and apply pagination manually
        var allProducts = await _unitOfWork.Products.GetAllAsync();
        var filtered = allProducts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            var searchTerm = paginationParams.SearchTerm.ToLower();
            filtered = filtered.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.SKU.ToLower().Contains(searchTerm) ||
                (p.Barcode != null && p.Barcode.ToLower().Contains(searchTerm)) ||
                (p.Brand != null && p.Brand.ToLower().Contains(searchTerm))
            );
        }

        // Apply sorting
        IOrderedQueryable<Product> sorted = paginationParams.SortBy?.ToLower() switch
        {
            "name" => paginationParams.SortDescending
                ? filtered.OrderByDescending(p => p.Name)
                : filtered.OrderBy(p => p.Name),
            "sku" => paginationParams.SortDescending
                ? filtered.OrderByDescending(p => p.SKU)
                : filtered.OrderBy(p => p.SKU),
            "price" => paginationParams.SortDescending
                ? filtered.OrderByDescending(p => p.Price)
                : filtered.OrderBy(p => p.Price),
            "stock" => paginationParams.SortDescending
                ? filtered.OrderByDescending(p => p.StockQuantity)
                : filtered.OrderBy(p => p.StockQuantity),
            "createdat" => paginationParams.SortDescending
                ? filtered.OrderByDescending(p => p.CreatedAt)
                : filtered.OrderBy(p => p.CreatedAt),
            _ => filtered.OrderBy(p => p.Id)
        };

        var count = sorted.Count();
        var products = sorted
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var productDtos = await MapToDtosAsync(products);

        return new PaginatedResult<ProductDto>(productDtos, count, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
    {
        var products = await _unitOfWork.Products.FindAsync(p => p.CategoryId == categoryId && p.IsActive);
        return await MapToDtosAsync(products);
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _unitOfWork.Products.FindAsync(p => p.StockQuantity <= p.MinStockLevel && p.IsActive);
        return await MapToDtosAsync(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return null;

        return await MapToDtoAsync(product);
    }

    public async Task<ProductDto?> GetProductByBarcodeAsync(string barcode)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Barcode == barcode);
        if (product == null) return null;

        return await MapToDtoAsync(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // Get the category first
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId);
        if (category == null)
            throw new ArgumentException($"Category with ID {dto.CategoryId} not found.");

        // Create product using constructor
        var product = new Product(dto.Name, dto.SKU, category, dto.Price, dto.CostPrice);

        // Update additional details
        product.UpdateDetails(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.CostPrice,
            dto.MinStockLevel,
            dto.MaxStockLevel,
            true, // IsActive = true for new products
            dto.ProductType,
            dto.Brand,
            dto.Unit,
            dto.Weight,
            dto.Location,
            dto.DirectSalesEnabled,
            dto.PointsEnabled
        );

        // Set initial stock
        if (dto.StockQuantity > 0)
        {
            product.UpdateStock(dto.StockQuantity);
        }

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(product);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return null;

        // Get current values to fill in what wasn't updated
        var name = dto.Name ?? product.Name;
        var description = dto.Description ?? product.Description;
        var price = dto.Price ?? product.Price;
        var costPrice = dto.CostPrice ?? product.CostPrice;
        var minStockLevel = dto.MinStockLevel ?? product.MinStockLevel;
        var maxStockLevel = dto.MaxStockLevel ?? product.MaxStockLevel;
        var isActive = dto.IsActive ?? product.IsActive;
        var productType = dto.ProductType ?? product.ProductType;
        var brand = dto.Brand ?? product.Brand;
        var unit = dto.Unit ?? product.Unit;
        var weight = dto.Weight ?? product.Weight;
        var location = dto.Location ?? product.Location;
        var directSalesEnabled = dto.DirectSalesEnabled ?? product.DirectSalesEnabled;
        var pointsEnabled = dto.PointsEnabled ?? product.PointsEnabled;

        // Update using the entity method
        product.UpdateDetails(
            name,
            description,
            price,
            costPrice,
            minStockLevel,
            maxStockLevel,
            isActive,
            productType,
            brand,
            unit,
            weight,
            location,
            directSalesEnabled,
            pointsEnabled
        );

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(product);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return false;

        // Soft delete by deactivating
        product.UpdateDetails(
            product.Name,
            product.Description,
            product.Price,
            product.CostPrice,
            product.MinStockLevel,
            product.MaxStockLevel,
            false, // IsActive = false
            product.ProductType,
            product.Brand,
            product.Unit,
            product.Weight,
            product.Location,
            product.DirectSalesEnabled,
            product.PointsEnabled
        );

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = await _unitOfWork.Products.FindAsync(p =>
            p.IsActive && (
                p.Name.Contains(searchTerm) ||
                p.SKU.Contains(searchTerm) ||
                (p.Barcode != null && p.Barcode.Contains(searchTerm))
            )
        );

        return await MapToDtosAsync(products);
    }

    private async Task<ProductDto> MapToDtoAsync(Product product)
    {
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(c => c.Id == product.CategoryId);

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
            CategoryName = category?.Name ?? string.Empty,
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

    private async Task<IEnumerable<ProductDto>> MapToDtosAsync(IEnumerable<Product> products)
    {
        var productList = products.ToList();
        if (!productList.Any())
            return Enumerable.Empty<ProductDto>();

        // Get all unique category IDs
        var categoryIds = productList.Select(p => p.CategoryId).Distinct().ToList();

        // Fetch all categories in one call
        var allCategories = await _unitOfWork.Categories.GetAllAsync();
        var categoryDict = allCategories.ToDictionary(c => c.Id, c => c.Name);

        return productList.Select(product => new ProductDto
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
            CategoryName = categoryDict.TryGetValue(product.CategoryId, out var categoryName) ? categoryName : string.Empty,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            ProductType = product.ProductType,
            Brand = product.Brand,
            Unit = product.Unit,
            Weight = product.Weight,
            Location = product.Location,
            DirectSalesEnabled = product.DirectSalesEnabled,
            PointsEnabled = product.PointsEnabled
        }).ToList();
    }
}
