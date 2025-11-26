using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class TransactionItem : Entity
{
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Transaction Transaction { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
