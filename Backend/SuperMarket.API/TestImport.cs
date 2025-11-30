// This is a test console application to import the Excel file directly
// To use this, create a new Console project or integrate into the API

using SuperMarket.Infrastructure.Data;
using SuperMarket.Infrastructure.Services;
using SuperMarket.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SuperMarket.API;

public class TestImport
{
    public static async Task Main()
    {
        var connectionString = "Host=localhost;Port=5432;Database=SuperMarketDb;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<SuperMarketDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        using var context = new SuperMarketDbContext(options);
        
        // Create a mock SKU generator or use the real one with proper DI
        // For testing purposes, we'll create dependencies manually
        var unitOfWork = new SuperMarket.Infrastructure.Repositories.UnitOfWork(context);
        var skuGenerator = new SuperMarket.Infrastructure.Services.SkuGeneratorService(unitOfWork);
        var importService = new ExcelImportService(context, skuGenerator);

        var filePath = @"C:\Users\lamtp\RiderProjects\Lam\LocalServices\SuperMarket\DanhSachSanPham_KV17102025-132919-053.xlsx";

        Console.WriteLine($"Starting import from: {filePath}");
        Console.WriteLine(new string('-', 60));

        try
        {
            var result = await importService.ImportProductsFromExcel(filePath);

            Console.WriteLine("Import completed!");
            Console.WriteLine($"Success: {result.Success}");
            Console.WriteLine($"Imported: {result.Imported}");
            Console.WriteLine($"Updated: {result.Updated}");
            Console.WriteLine($"Skipped: {result.Skipped}");
            Console.WriteLine($"Summary: {result.Summary}");

            if (result.Errors.Any())
            {
                Console.WriteLine($"\nErrors ({result.Errors.Count}):");
                foreach (var error in result.Errors.Take(20))
                {
                    Console.WriteLine($"  - {error}");
                }
                if (result.Errors.Count > 20)
                {
                    Console.WriteLine($"  ... and {result.Errors.Count - 20} more errors");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during import: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
