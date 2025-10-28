using OfficeOpenXml;
using SuperMarket.API.Models;
using SuperMarket.API.Data;
using Microsoft.EntityFrameworkCore;

namespace SuperMarket.API.Services;

public interface IExcelImportService
{
    Task<ExcelImportResult> ImportProductsFromExcel(string filePath);
}

public class ExcelImportService : IExcelImportService
{
    private readonly SuperMarketDbContext _context;

    public ExcelImportService(SuperMarketDbContext context)
    {
        _context = context;
    }

    public async Task<ExcelImportResult> ImportProductsFromExcel(string filePath)
    {
        var result = new ExcelImportResult();

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet.Dimension == null)
        {
            result.Errors.Add("The Excel file is empty.");
            return result;
        }

        var rowCount = worksheet.Dimension.Rows;

        // Get or create default category
        var defaultCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Uncategorized");
        if (defaultCategory == null)
        {
            defaultCategory = new Category { Name = "Uncategorized", Description = "Auto-created during import" };
            _context.Categories.Add(defaultCategory);
            await _context.SaveChangesAsync();
        }

        // Cache for categories
        var categoryCache = new Dictionary<string, Category>();

        for (int row = 2; row <= rowCount; row++) // Skip header row
        {
            try
            {
                var loaiHang = worksheet.Cells[row, 1].GetValue<string>() ?? string.Empty;
                var nhomHang = worksheet.Cells[row, 2].GetValue<string>() ?? string.Empty;
                var maHang = worksheet.Cells[row, 3].GetValue<string>() ?? string.Empty;
                var maVach = worksheet.Cells[row, 4].GetValue<string>() ?? string.Empty;
                var tenHang = worksheet.Cells[row, 5].GetValue<string>() ?? string.Empty;
                var thuongHieu = worksheet.Cells[row, 6].GetValue<string>() ?? string.Empty;
                var giaBan = worksheet.Cells[row, 7].GetValue<decimal?>() ?? 0;
                var giaVon = worksheet.Cells[row, 8].GetValue<decimal?>() ?? 0;
                var tonKho = worksheet.Cells[row, 9].GetValue<int?>() ?? 0;
                var tonNhoNhat = worksheet.Cells[row, 13].GetValue<int?>() ?? 10;
                var tonLonNhat = worksheet.Cells[row, 14].GetValue<int?>() ?? null;
                var dvt = worksheet.Cells[row, 15].GetValue<string>() ?? string.Empty;
                var trongLuong = worksheet.Cells[row, 22].GetValue<decimal?>() ?? null;
                var tichDiem = worksheet.Cells[row, 23].GetValue<int?>() ?? 0;
                var dangKinhDoanh = worksheet.Cells[row, 24].GetValue<int?>() ?? 1;
                var duocBanTrucTiep = worksheet.Cells[row, 25].GetValue<int?>() ?? 1;
                var moTa = worksheet.Cells[row, 26].GetValue<string>() ?? string.Empty;
                var viTri = worksheet.Cells[row, 28].GetValue<string>() ?? string.Empty;
                var hinhAnh = worksheet.Cells[row, 20].GetValue<string>() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(maHang) || string.IsNullOrWhiteSpace(tenHang))
                {
                    result.Skipped++;
                    result.Errors.Add($"Row {row}: Missing required fields (SKU or Name)");
                    continue;
                }

                // Get or create category
                Category category;
                if (!string.IsNullOrWhiteSpace(nhomHang))
                {
                    if (!categoryCache.TryGetValue(nhomHang, out category))
                    {
                        category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == nhomHang);
                        if (category == null)
                        {
                            category = new Category { Name = nhomHang, Description = $"Auto-created from Excel import" };
                            _context.Categories.Add(category);
                            await _context.SaveChangesAsync();
                        }
                        categoryCache[nhomHang] = category;
                    }
                }
                else
                {
                    category = defaultCategory;
                }

                // Check if product exists
                var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == maHang);

                if (existingProduct != null)
                {
                    // Update existing product
                    existingProduct.Name = tenHang;
                    existingProduct.Description = moTa;
                    existingProduct.Barcode = maVach;
                    existingProduct.Price = giaBan;
                    existingProduct.CostPrice = giaVon;
                    existingProduct.StockQuantity = tonKho;
                    existingProduct.MinStockLevel = tonNhoNhat;
                    existingProduct.MaxStockLevel = tonLonNhat;
                    existingProduct.CategoryId = category.Id;
                    existingProduct.ImageUrl = hinhAnh;
                    existingProduct.IsActive = dangKinhDoanh == 1;
                    existingProduct.ProductType = loaiHang;
                    existingProduct.Brand = thuongHieu;
                    existingProduct.Unit = dvt;
                    existingProduct.Weight = trongLuong;
                    existingProduct.Location = viTri;
                    existingProduct.DirectSalesEnabled = duocBanTrucTiep == 1;
                    existingProduct.PointsEnabled = tichDiem == 1;
                    existingProduct.UpdatedAt = DateTime.UtcNow;

                    result.Updated++;
                }
                else
                {
                    // Create new product
                    var product = new Product
                    {
                        SKU = maHang,
                        Name = tenHang,
                        Description = moTa,
                        Barcode = maVach,
                        Price = giaBan,
                        CostPrice = giaVon,
                        StockQuantity = tonKho,
                        MinStockLevel = tonNhoNhat,
                        MaxStockLevel = tonLonNhat,
                        CategoryId = category.Id,
                        ImageUrl = hinhAnh,
                        IsActive = dangKinhDoanh == 1,
                        ProductType = loaiHang,
                        Brand = thuongHieu,
                        Unit = dvt,
                        Weight = trongLuong,
                        Location = viTri,
                        DirectSalesEnabled = duocBanTrucTiep == 1,
                        PointsEnabled = tichDiem == 1,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Products.Add(product);
                    result.Imported++;
                }
            }
            catch (Exception ex)
            {
                result.Skipped++;
                result.Errors.Add($"Row {row}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync();
        result.Success = true;

        return result;
    }
}

public class ExcelImportResult
{
    public bool Success { get; set; }
    public int Imported { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();

    public string Summary => $"Imported: {Imported}, Updated: {Updated}, Skipped: {Skipped}, Errors: {Errors.Count}";
}
