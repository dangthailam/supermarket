using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class ProductBarcode : Entity
{
    public Guid ProductId { get; private set; }
    public string Barcode { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }

    // Navigation property
    public Product Product { get; private set; } = null!;

    // Parameterless constructor for EF Core
    private ProductBarcode()
    {
    }

    public ProductBarcode(Guid productId, string barcode, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new ArgumentException("Barcode cannot be null or empty", nameof(barcode));

        ProductId = productId;
        Barcode = barcode.Trim();
        IsPrimary = isPrimary;
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetPrimary()
    {
        IsPrimary = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
