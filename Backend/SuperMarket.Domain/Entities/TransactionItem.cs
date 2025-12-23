using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class TransactionItem : Entity
{
    public Guid TransactionId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Transaction Transaction { get; set; } = null!;
    public Product Product { get; set; } = null!;

    // Private constructor for EF Core
    private TransactionItem() { }

    // Factory method to create a transaction item
    public static TransactionItem Create(Product product, int quantity, decimal discount)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (discount < 0)
            throw new ArgumentException("Discount cannot be negative", nameof(discount));

        var totalPrice = (product.Price * quantity) - discount;

        var item = new TransactionItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = quantity,
            UnitPrice = product.Price,
            Discount = discount,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.UtcNow
        };

        return item;
    }

    // Set the transaction ID (called after transaction is saved)
    public void SetTransactionId(Guid transactionId)
    {
        if (transactionId == Guid.Empty)
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));

        TransactionId = transactionId;
    }
}
