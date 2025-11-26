using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class Transaction : Entity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Mobile
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid? UserId { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
}

public enum TransactionStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2,
    Refunded = 3
}
