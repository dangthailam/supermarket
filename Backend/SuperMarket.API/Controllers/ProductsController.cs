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
    private readonly Supabase.Client _supabaseClient;
    private readonly IConfiguration _configuration;

    public ProductsController(IProductService productService, Supabase.Client supabaseClient, IConfiguration configuration)
    {
        _productService = productService;
        _supabaseClient = supabaseClient;
        _configuration = configuration;
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
    public async Task<ActionResult> UploadProductImage(IFormFile productImage)
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
            // Generate filename based on file content hash to prevent duplicates
            var fileExtension = Path.GetExtension(productImage.FileName);
            
            // Read file into byte array
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await productImage.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // Generate hash from file content - same file = same hash = no duplicate
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(fileBytes);
            var fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower().Substring(0, 16);
            var fileName = $"{fileHash}{fileExtension}";

            // Get storage bucket name from configuration
            var bucketName = _configuration.GetSection("Supabase:StorageBucket").Value ?? "product-images";
            
            // Upload to Supabase Storage with Upsert=true to replace if exists
            await _supabaseClient.Storage
                .From(bucketName)
                .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                {
                    ContentType = productImage.ContentType,
                    Upsert = true
                });

            // Get public URL
            var publicUrl = _supabaseClient.Storage
                .From(bucketName)
                .GetPublicUrl(fileName);
            
            return Ok(new { imageUrl = publicUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to upload image", error = ex.Message });
        }
    }
}
