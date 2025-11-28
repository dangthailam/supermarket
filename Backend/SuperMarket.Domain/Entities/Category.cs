using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class Category : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Self-referencing relationship for hierarchy
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = [];

    // Navigation property
    public ICollection<Product> Products { get; set; } = [];

    public void AddProduct(Product product)
    {
        Products.Add(product);
    }

    public void RemoveProduct(Product product)
    {
        Products.Remove(product);
    }
}
