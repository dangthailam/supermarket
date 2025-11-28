using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class Brand : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = [];
}