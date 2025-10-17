// This is a test console application to import the Excel file directly
// To use this, create a new Console project or integrate into the API

using SuperMarket.API.Data;
using SuperMarket.API.Services;
using Microsoft.EntityFrameworkCore;

namespace SuperMarket.API;

public class TestImport
{
    public static async Task Main()
    {
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=SuperMarketDb;Trusted_Connection=true;TrustServerCertificate=true;";

        var options = new DbContextOptionsBuilder<SuperMarketDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new SuperMarketDbContext(options);
        var importService = new ExcelImportService(context);

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
