namespace SuperMarket.Application.Interfaces;

public interface ISkuGeneratorService
{
    /// <summary>
    /// Generates a unique SKU for a product based on category and product information
    /// </summary>
    /// <param name="categoryId">The category ID of the product</param>
    /// <param name="productName">The name of the product (optional, used for prefix)</param>
    /// <returns>A unique SKU string</returns>
    Task<string> GenerateSkuAsync(Guid categoryId, string? productName = null);

    /// <summary>
    /// Checks if a SKU already exists in the database
    /// </summary>
    /// <param name="sku">The SKU to check</param>
    /// <returns>True if SKU exists, false otherwise</returns>
    Task<bool> IsSkuExistsAsync(string sku);
}