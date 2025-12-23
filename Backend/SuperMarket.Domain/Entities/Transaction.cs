using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class Transaction : Entity
{
    private const decimal DefaultTaxRate = 0.10m; // 10%

    public string TransactionNumber { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? CustomerName { get; private set; }
    public string? CustomerPhone { get; private set; }
    public Guid? UserId { get; private set; }
    public TransactionStatus Status { get; private set; } = TransactionStatus.Completed;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<TransactionItem> TransactionItems { get; private set; } = new List<TransactionItem>();

    // Private constructor for EF Core
    private Transaction() { }

    // Factory method to create a new transaction
    public static Transaction Create(
        string paymentMethod,
        decimal discountAmount,
        string? customerName = null,
        string? customerPhone = null)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method is required", nameof(paymentMethod));

        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        var transaction = new Transaction
        {
            TransactionNumber = GenerateTransactionNumber(),
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = paymentMethod,
            DiscountAmount = discountAmount,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            Status = TransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        return transaction;
    }

    // Add item to transaction
    public TransactionItem AddItem(Product product, int quantity, decimal discount = 0)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (discount < 0)
            throw new ArgumentException("Discount cannot be negative", nameof(discount));

        if (Status != TransactionStatus.Completed)
            throw new InvalidOperationException("Cannot add items to a non-completed transaction");

        var item = TransactionItem.Create(product, quantity, discount);
        TransactionItems.Add(item);

        return item;
    }

    // Calculate totals for the transaction
    public void CalculateTotals()
    {
        TotalAmount = TransactionItems.Sum(i => i.TotalPrice);
        TaxAmount = TotalAmount * DefaultTaxRate;
        NetAmount = TotalAmount + TaxAmount - DiscountAmount;
    }

    // Cancel the transaction
    public void Cancel()
    {
        if (Status == TransactionStatus.Cancelled)
            throw new InvalidOperationException("Transaction is already cancelled");

        if (Status == TransactionStatus.Refunded)
            throw new InvalidOperationException("Cannot cancel a refunded transaction");

        Status = TransactionStatus.Cancelled;
    }

    // Check if transaction can be cancelled
    public bool CanBeCancelled()
    {
        return Status == TransactionStatus.Completed;
    }

    // Get items that need stock restoration (for cancellation)
    public IEnumerable<(Guid ProductId, int Quantity)> GetItemsForStockRestoration()
    {
        return TransactionItems.Select(i => (i.ProductId, i.Quantity));
    }

    private static string GenerateTransactionNumber()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}

public enum TransactionStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2,
    Refunded = 3
}
