using SuperMarket.Application.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace SuperMarket.Infrastructure.Services;

public class SkuGeneratorService : ISkuGeneratorService
{
    private readonly IUnitOfWork _unitOfWork;

    public SkuGeneratorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GenerateSkuAsync(Guid categoryId, string? productName = null)
    {
        // Get category to build category prefix
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        var categoryPrefix = BuildCategoryPrefix(category);

        // Get product name prefix (first 3 chars of cleaned product name)
        var productPrefix = BuildProductPrefix(productName);

        // Get next sequential number for this category
        var sequentialNumber = await GetNextSequentialNumberAsync(categoryPrefix);

        // Format: [CATEGORY][PRODUCT][NNNN]
        // Example: ELC-LAP-0001 (Electronics-Laptop-0001)
        var baseSku = $"{categoryPrefix}-{productPrefix}-{sequentialNumber:D4}";

        // Ensure uniqueness by checking database
        var finalSku = await EnsureUniqueSkuAsync(baseSku);

        return finalSku;
    }

    public async Task<bool> IsSkuExistsAsync(string sku)
    {
        return await _unitOfWork.Products.AnyAsync(p => p.SKU == sku);
    }

    private string BuildCategoryPrefix(Domain.Entities.Category? category)
    {
        if (category == null)
        {
            return "GEN"; // General category prefix
        }

        // Clean and abbreviate category name
        var cleaned = CleanString(category.Name);
        
        // Take first 3 characters or pad if shorter
        if (cleaned.Length >= 3)
        {
            return cleaned.Substring(0, 3).ToUpper();
        }

        return cleaned.PadRight(3, 'X').ToUpper();
    }

    private string BuildProductPrefix(string? productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return "PRD"; // Default product prefix
        }

        var cleaned = CleanString(productName);
        
        // Take first 3 characters or pad if shorter
        if (cleaned.Length >= 3)
        {
            return cleaned.Substring(0, 3).ToUpper();
        }

        return cleaned.PadRight(3, 'X').ToUpper();
    }

    private string CleanString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Remove special characters, spaces, and numbers, keep only letters
        var cleaned = Regex.Replace(input, @"[^a-zA-Z]", "");
        
        // If no letters remain, return a default
        return string.IsNullOrEmpty(cleaned) ? "XXX" : cleaned;
    }

    private async Task<int> GetNextSequentialNumberAsync(string categoryPrefix)
    {
        // Get all products with SKUs starting with this category prefix
        var existingProducts = await _unitOfWork.Products.FindAsync(p => 
            p.SKU.StartsWith(categoryPrefix + "-"));

        if (!existingProducts.Any())
        {
            return 1;
        }

        // Extract sequential numbers from existing SKUs
        var maxSequence = 0;
        var pattern = $@"{Regex.Escape(categoryPrefix)}-[A-Z]{{3}}-(\d{{4}})";

        foreach (var product in existingProducts)
        {
            var match = Regex.Match(product.SKU, pattern);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var sequence))
            {
                maxSequence = Math.Max(maxSequence, sequence);
            }
        }

        return maxSequence + 1;
    }

    private async Task<string> EnsureUniqueSkuAsync(string baseSku)
    {
        var currentSku = baseSku;
        var counter = 1;

        // Check if the base SKU already exists
        while (await IsSkuExistsAsync(currentSku))
        {
            // If it exists, append a suffix and try again
            // Format: BASE-SKU-X1, BASE-SKU-X2, etc.
            currentSku = $"{baseSku}-X{counter}";
            counter++;

            // Safety limit to prevent infinite loop
            if (counter > 999)
            {
                // Use timestamp as final fallback
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString().Substring(5);
                currentSku = $"{baseSku}-T{timestamp}";
                break;
            }
        }

        return currentSku;
    }
}