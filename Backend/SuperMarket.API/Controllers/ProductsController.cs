using Microsoft.AspNetCore.Mvc;
using SuperMarket.Application.DTOs;
using SuperMarket.Application.Services;
using SuperMarket.Domain.Common;

namespace SuperMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> GetProductsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _productService.GetProductsPagedAsync(paginationParams);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        
        if (product == null)
            return NotFound();
        
        return Ok(product);
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ProductDto>> GetProductByBarcode(string barcode)
    {
        var product = await _productService.GetProductByBarcodeAsync(barcode);
        
        if (product == null)
            return NotFound();
        
        return Ok(product);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(Guid categoryId)
    {
        var products = await _productService.GetProductsByCategoryAsync(categoryId);
        return Ok(products);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
    {
        var products = await _productService.GetLowStockProductsAsync();
        return Ok(products);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required");

        var products = await _productService.SearchProductsAsync(searchTerm);
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var product = await _productService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateProductAsync(id, dto);
        
        if (product == null)
            return NotFound();
        
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var success = await _productService.DeleteProductAsync(id);
        
        if (!success)
            return NotFound();
        
        return NoContent();
    }

    [HttpPost("upload-image")]
    public async Task<ActionResult<object>> UploadProductImage(IFormFile productImage)
    {
        if (productImage == null || productImage.Length == 0)
            return BadRequest(new { message = "No file provided" });

        // Validate file type
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedMimeTypes.Contains(productImage.ContentType.ToLower()))
            return BadRequest(new { message = "Only image files are allowed (JPG, PNG, GIF, WEBP)" });

        // Validate file size (5MB max)
        if (productImage.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "File size must be less than 5MB" });

        try
        {
            // Generate unique filename
            var fileName = $"products/{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(productImage.FileName)}";
            
            // Save file to blob storage
            // For now, we'll save to local folder. Replace this with Azure Blob Storage later
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);
            
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await productImage.CopyToAsync(stream);
            }

            // Return the image URL (you'll replace /uploads with your blob storage URL)
            var imageUrl = $"/uploads/{fileName}";
            
            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to upload image", error = ex.Message });
        }
    }
}
