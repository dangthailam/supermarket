namespace SuperMarket.Application.Interfaces;

public interface IExcelImportService
{
    Task<ExcelImportResult> ImportProductsFromExcel(string filePath);
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
