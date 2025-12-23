using FluentAssertions;
using SuperMarket.Domain.Entities;

namespace SuperMarket.Domain.Tests.Entities;

public class TransactionTests
{
    [Fact]
    public void Create_ShouldCreateTransactionWithValidData()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var paymentMethod = "Cash";
        var discountAmount = 50m;
        var customerName = "John Doe";
        var customerPhone = "123456789";

        // Act
        var transaction = Transaction.Create(paymentMethod, discountAmount, customerName, customerPhone);

        // Assert
        transaction.Should().NotBeNull();
        transaction.PaymentMethod.Should().Be(paymentMethod);
        transaction.DiscountAmount.Should().Be(discountAmount);
        transaction.CustomerName.Should().Be(customerName);
        transaction.CustomerPhone.Should().Be(customerPhone);
        transaction.Status.Should().Be(TransactionStatus.Completed);
        transaction.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenPaymentMethodIsEmpty()
    {
        // Act & Assert
        var act = () => Transaction.Create("", 0, "Customer", "123");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDiscountAmountIsNegative()
    {
        // Act & Assert
        var act = () => Transaction.Create("Cash", -10, "Customer", "123");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_ShouldAddItemToTransaction()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var transaction = Transaction.Create("Cash", 0, "Customer", "123");
        product.UpdateStock(10);

        // Act
        transaction.AddItem(product, 2, 50m);

        // Assert
        transaction.TransactionItems.Should().HaveCount(1);
        var item = transaction.TransactionItems.First();
        item.ProductId.Should().Be(product.Id);
        item.ProductName.Should().Be(product.Name);
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(product.Price);
        item.Discount.Should().Be(50m);
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenProductIsNull()
    {
        // Arrange
        var transaction = Transaction.Create("Cash", 0, "Customer", "123");

        // Act & Assert
        var act = () => transaction.AddItem(null!, 1, 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenQuantityIsZero()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var transaction = Transaction.Create("Cash", 0, "Customer", "123");

        // Act & Assert
        var act = () => transaction.AddItem(product, 0, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculateTotals_ShouldCalculateCorrectAmounts()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product1 = new Product("Laptop", category, 1000m, 800m);
        var product2 = new Product("Mouse", category, 50m, 30m);
        var transaction = Transaction.Create("Cash", 100m, "Customer", "123");
        
        product1.UpdateStock(10);
        product2.UpdateStock(20);
        
        transaction.AddItem(product1, 1, 0);   // 1000
        transaction.AddItem(product2, 2, 0);   // 100

        // Act
        transaction.CalculateTotals();

        // Assert
        transaction.TotalAmount.Should().Be(1100m);           // 1000 + 100
        transaction.TaxAmount.Should().Be(110m);              // 10% of 1100
        transaction.NetAmount.Should().Be(1110m);             // 1100 + 110 - 100 discount
    }

    [Fact]
    public void Cancel_ShouldCancelTransaction_WhenStatusIsCompleted()
    {
        // Arrange
        var transaction = Transaction.Create("Cash", 0, "Customer", "123");

        // Act
        transaction.Cancel();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenAlreadyCancelled()
    {
        // Arrange
        var transaction = Transaction.Create("Cash", 0, "Customer", "123");
        transaction.Cancel();

        // Act & Assert
        var act = () => transaction.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CanBeCancelled_ShouldReturnTrue_WhenStatusIsCompleted()
    {
        // Arrange
        var transaction = Transaction.Create("Cash", 0, "Customer", "123");

        // Act
        var canBeCancelled = transaction.CanBeCancelled();

        // Assert
        canBeCancelled.Should().BeTrue();
    }

    [Fact]
    public void CanBeCancelled_ShouldReturnFalse_WhenStatusIsCancelled()
    {
        // Arrange
        var transaction = Transaction.Create("Cash", 0, "Customer", "123");
        transaction.Cancel();

        // Act
        var canBeCancelled = transaction.CanBeCancelled();

        // Assert
        canBeCancelled.Should().BeFalse();
    }

    [Fact]
    public void GetItemsForStockRestoration_ShouldReturnAllItems()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product1 = new Product("Laptop", category, 1000m, 800m);
        var product2 = new Product("Mouse", category, 50m, 30m);
        var transaction = Transaction.Create("Cash", 0, "Customer", "123");
        
        product1.UpdateStock(10);
        product2.UpdateStock(20);
        
        transaction.AddItem(product1, 2, 0);
        transaction.AddItem(product2, 3, 0);

        // Act
        var items = transaction.GetItemsForStockRestoration();

        // Assert
        items.Should().HaveCount(2);
        items.Should().Contain(item => item.ProductId == product1.Id && item.Quantity == 2);
        items.Should().Contain(item => item.ProductId == product2.Id && item.Quantity == 3);
    }
}
