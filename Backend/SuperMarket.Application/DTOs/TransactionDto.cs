using SuperMarket.Domain.Entities;

namespace SuperMarket.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public TransactionStatus Status { get; set; }
    public List<TransactionItemDto> Items { get; set; } = new();
}

public class TransactionItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateTransactionDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<CreateTransactionItemDto> Items { get; set; } = new();
}

public class CreateTransactionItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
}
