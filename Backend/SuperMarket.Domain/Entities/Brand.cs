using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.Entities;

public class Brand : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<Product> Products { get; private set; } = [];

    public Brand(string name, string description)
    {
        Name = name;
        Description = description;
    }

    private Brand() { } // For EF Core
}