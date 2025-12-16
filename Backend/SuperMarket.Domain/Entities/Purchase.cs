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

    /// <summary>
    /// Marks the purchase as Paid and updates product inventory.
    /// Only valid when current status is Pending.
    /// </summary>
    public void MarkAsPaid()
    {
        if (Status == PurchaseStatus.Paid)
            return; // Already paid

        if (Status != PurchaseStatus.Pending)
            throw new InvalidOperationException($"Cannot mark purchase as Paid from {Status} status.");

        Status = PurchaseStatus.Paid;
        AddProductsToInventory();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reverts the purchase status from Paid back to Pending and removes items from inventory.
    /// Only valid when current status is Paid.
    /// </summary>
    public void RevertFromPaid()
    {
        if (Status != PurchaseStatus.Paid)
            throw new InvalidOperationException($"Cannot revert purchase from {Status} status. Only Paid purchases can be reverted.");

        RemoveProductsFromInventory();
        Status = PurchaseStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the purchase. Removes inventory if it was already paid.
    /// </summary>
    public void Cancel()
    {
        if (Status == PurchaseStatus.Cancelled)
            return; // Already cancelled

        if (Status == PurchaseStatus.Paid)
        {
            RemoveProductsFromInventory();
        }

        Status = PurchaseStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates purchase metadata and items.
    /// </summary>
    public void UpdatePurchase(DateTime purchaseDate, Provider provider, string? note, ICollection<PurchaseItem> newItems, PurchaseStatus? status)
    {
        PurchaseDate = purchaseDate;
        ProviderId = provider.Id;
        Provider = provider;
        Note = note;
        PurchaseItems = newItems;
        UpdatedAt = DateTime.UtcNow;

        if (status.HasValue)
        {
            TransitionToStatus(status.Value);
        }
    }

    /// <summary>
    /// Transitions purchase to a new status.
    /// Handles all side effects like inventory updates.
    /// </summary>
    public void TransitionToStatus(PurchaseStatus newStatus)
    {
        if (Status == newStatus)
            return; // No change

        switch (newStatus)
        {
            case PurchaseStatus.Paid:
                MarkAsPaid();
                break;
            case PurchaseStatus.Pending:
                RevertFromPaid();
                break;
            case PurchaseStatus.Cancelled:
                Cancel();
                break;
        }
    }

    private void AddProductsToInventory()
    {
        foreach (var item in PurchaseItems)
        {
            item.Product.UpdateStock(item.Quantity);
        }
    }

    private void RemoveProductsFromInventory()
    {
        foreach (var item in PurchaseItems)
        {
            item.Product.UpdateStock(-item.Quantity);
        }
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

    public void Update(int quantity, decimal purchasePrice, decimal? discount = null, string? note = null)
    {
        Quantity = quantity;
        PurchasePrice = purchasePrice;
        Discount = discount;
        Note = note;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Domain service for purchase code generation.
/// Encapsulates the logic for generating unique purchase codes.
/// </summary>
public class PurchaseCodeGenerator
{
    /// <summary>
    /// Generates a unique purchase code based on the current date and existing purchases.
    /// Format: POyyyyMMddNNNN (e.g., PO202512090001)
    /// </summary>
    public static string GenerateCode(IEnumerable<Purchase> existingPurchases)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PO{today:yyyyMMdd}";

        var todayPurchases = existingPurchases
            .Where(p => p.Code.StartsWith(prefix))
            .ToList();

        var maxNumber = 0;
        foreach (var purchase in todayPurchases)
        {
            if (purchase.Code.Length > prefix.Length)
            {
                var numberPart = purchase.Code.Substring(prefix.Length);
                if (int.TryParse(numberPart, out var number))
                {
                    maxNumber = Math.Max(maxNumber, number);
                }
            }
        }

        return $"{prefix}{(maxNumber + 1):D4}";
    }
}