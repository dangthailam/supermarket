using SuperMarket.Domain.Entities;

namespace SuperMarket.Application.DTOs;

public class PurchaseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderCode { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseItemDto> Items { get; set; } = [];
}

public class PurchaseItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal? Discount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Note { get; set; }
}

public class CreatePurchaseDto
{
    public DateTime PurchaseDate { get; set; }
    public Guid ProviderId { get; set; }
    public PurchaseStatus Status { get; set; }
    public string? Note { get; set; }
    public List<CreatePurchaseItemDto> Items { get; set; } = [];
}

public class CreatePurchaseItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal? Discount { get; set; }
    public string? Note { get; set; }
}

public class UpdatePurchaseDto
{
    public DateTime? PurchaseDate { get; set; }
    public Guid? ProviderId { get; set; }
    public PurchaseStatus? Status { get; set; }
    public string? Note { get; set; }
    public List<CreatePurchaseItemDto> Items { get; set; } = [];
}
