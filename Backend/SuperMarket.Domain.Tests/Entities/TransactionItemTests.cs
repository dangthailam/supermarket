using FluentAssertions;
using SuperMarket.Domain.Entities;

namespace SuperMarket.Domain.Tests.Entities;

public class TransactionItemTests
{
    [Fact]
    public void Create_ShouldCreateTransactionItemWithValidData()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var quantity = 2;
        var discount = 100m;

        // Act
        var item = TransactionItem.Create(product, quantity, discount);

        // Assert
        item.Should().NotBeNull();
        item.ProductId.Should().Be(product.Id);
        item.ProductName.Should().Be(product.Name);
        item.Quantity.Should().Be(quantity);
        item.UnitPrice.Should().Be(product.Price);
        item.Discount.Should().Be(discount);
        item.TotalPrice.Should().Be((product.Price * quantity) - discount);
    }

    [Fact]
    public void Create_ShouldCalculateTotalPriceCorrectly()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Keyboard", category, 50m, 30m);
        var quantity = 5;
        var discount = 25m;

        // Act
        var item = TransactionItem.Create(product, quantity, discount);

        // Assert
        item.TotalPrice.Should().Be(225m); // (50 * 5) - 25
    }

    [Fact]
    public void Create_ShouldThrowException_WhenProductIsNull()
    {
        // Act & Assert
        var act = () => TransactionItem.Create(null!, 1, 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenQuantityIsZero()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);

        // Act & Assert
        var act = () => TransactionItem.Create(product, 0, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenQuantityIsNegative()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);

        // Act & Assert
        var act = () => TransactionItem.Create(product, -1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDiscountIsNegative()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);

        // Act & Assert
        var act = () => TransactionItem.Create(product, 1, -10m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetTransactionId_ShouldSetTransactionId()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var item = TransactionItem.Create(product, 1, 0);
        var transactionId = Guid.NewGuid();

        // Act
        item.SetTransactionId(transactionId);

        // Assert
        item.TransactionId.Should().Be(transactionId);
    }

    [Fact]
    public void SetTransactionId_ShouldThrowException_WhenTransactionIdIsEmpty()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var item = TransactionItem.Create(product, 1, 0);

        // Act & Assert
        var act = () => item.SetTransactionId(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldHandleZeroDiscount()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Monitor", category, 75m, 50m);
        var quantity = 3;

        // Act
        var item = TransactionItem.Create(product, quantity, 0);

        // Assert
        item.TotalPrice.Should().Be(225m); // 75 * 3
        item.Discount.Should().Be(0);
    }
}
