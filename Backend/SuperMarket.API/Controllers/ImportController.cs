using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperMarket.Application.Interfaces;

namespace SuperMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportController : ControllerBase
{
    private readonly IExcelImportService _excelImportService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(IExcelImportService excelImportService, ILogger<ImportController> logger)
    {
        _excelImportService = excelImportService;
        _logger = logger;
    }

    [HttpPost("products/excel")]
    public async Task<ActionResult> ImportProductsFromExcel()
    {
        var filePath = @"C:\Users\lamtp\RiderProjects\Lam\LocalServices\SuperMarket\DanhSachSanPham_KV17102025-132919-053.xlsx";

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { message = $"File not found: {filePath}" });
        }

        try
        {
            _logger.LogInformation("Starting import from: {FilePath}", filePath);

            var result = await _excelImportService.ImportProductsFromExcel(filePath);

            _logger.LogInformation("Import completed. Imported: {Imported}, Updated: {Updated}, Errors: {Errors}", 
                result.Imported, result.Updated, result.Errors.Count);

            return Ok(new
            {
                success = result.Success,
                imported = result.Imported,
                updated = result.Updated,
                skipped = result.Skipped,
                errors = result.Errors,
                summary = result.Summary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing products from Excel");
            return BadRequest(new { message = ex.Message, error = ex.ToString() });
        }
    }

    [HttpPost("products/excel/upload")]
    public async Task<ActionResult> ImportProductsFromUploadedFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only .xlsx files are supported" });
        }

        try
        {
            // Save uploaded file to temp location
            var tempPath = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}.xlsx");
            
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Uploaded file saved to: {TempPath}", tempPath);

            var result = await _excelImportService.ImportProductsFromExcel(tempPath);

            // Clean up temp file
            System.IO.File.Delete(tempPath);

            _logger.LogInformation("Import completed. Imported: {Imported}, Updated: {Updated}, Errors: {Errors}", 
                result.Imported, result.Updated, result.Errors.Count);

            return Ok(new
            {
                success = result.Success,
                imported = result.Imported,
                updated = result.Updated,
                skipped = result.Skipped,
                errors = result.Errors,
                summary = result.Summary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing products from uploaded Excel file");
            return BadRequest(new { message = ex.Message, error = ex.ToString() });
        }
    }
}
