using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class Purchase : Entity
{
    public ICollection<PurchaseItem> PurchaseItems { get; private set; } = [];

    public DateTime PurchaseDate { get; private set; }

    public string Code { get; private set; }

    public Guid ProviderId { get; private set; }
    public Provider Provider { get; private set; }

    public PurchaseStatus Status { get; private set; }

    public string? Note { get; private set; }

    private Purchase()
    {
        Code = string.Empty;
        Provider = null!;
    }

    public Purchase(List<PurchaseItem> purchaseItems, DateTime purchaseDate, string code, Provider provider, PurchaseStatus status, string? note = null)
    {
        PurchaseItems = purchaseItems;
        PurchaseDate = purchaseDate;
        Code = code;
        ProviderId = provider.Id;
        Provider = provider;
        Status = status;
        Note = note;
    }
}

public enum PurchaseStatus
{
    Pending = 1,
    Paid = 2,
    Cancelled = 3
}

public class PurchaseItem : Entity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public decimal? Discount { get; private set; }

    public string? Note { get; private set; }

    private PurchaseItem()
    {
    }

    public PurchaseItem(Product product, int quantity, decimal purchasePrice, decimal? discount = null, string? note = null)
    {
        ProductId = product.Id;
        Quantity = quantity;
        PurchasePrice = purchasePrice;
        Discount = discount;
        Note = note;
    }
}