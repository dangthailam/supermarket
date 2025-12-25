using FluentAssertions;
using SuperMarket.Domain.Entities;

namespace SuperMarket.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldCreateProductWithValidData()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var name = "Laptop";
        var price = 1000m;
        var costPrice = 800m;

        // Act
        var product = new Product(name, category, price, costPrice);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(name);
        product.CategoryId.Should().Be(category.Id);
        product.Price.Should().Be(price);
        product.CostPrice.Should().Be(costPrice);
        product.StockQuantity.Should().Be(0);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateStock_ShouldIncreaseStock_WhenQuantityIsPositive()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var initialStock = product.StockQuantity;

        // Act
        product.UpdateStock(10);

        // Assert
        product.StockQuantity.Should().Be(initialStock + 10);
    }

    [Fact]
    public void UpdateStock_ShouldDecreaseStock_WhenQuantityIsNegative()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        product.UpdateStock(20);

        // Act
        product.UpdateStock(-5);

        // Assert
        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void SetSku_ShouldSetSkuValue()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var sku = "LAP-001";

        // Act
        product.SetSku(sku);

        // Assert
        product.SKU.Should().Be(sku);
    }

    [Fact]
    public void SetSku_ShouldThrowException_WhenSkuIsEmpty()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);

        // Act & Assert
        var act = () => product.SetSku("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddBarcode_ShouldAddBarcodeToProduct()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var barcodeValue = "1234567890123";

        // Act
        var barcode = product.AddBarcode(barcodeValue, true);

        // Assert
        barcode.Should().NotBeNull();
        barcode!.Barcode.Should().Be(barcodeValue);
        barcode.IsPrimary.Should().BeTrue();
        product.Barcodes.Should().HaveCount(1);
    }

    [Fact]
    public void AddBarcode_ShouldSetAsPrimary_WhenIsFirstBarcode()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);

        // Act
        var barcode = product.AddBarcode("1234567890123", true);

        // Assert
        barcode.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void AddBarcode_ShouldUnsetOtherPrimary_WhenAddingNewPrimary()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var firstBarcode = product.AddBarcode("1111111111111", true);

        // Act
        var secondBarcode = product.AddBarcode("2222222222222", true);

        // Assert
        firstBarcode.IsPrimary.Should().BeFalse();
        secondBarcode.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void AddBarcode_ShouldReturnNull_WhenBarcodeAlreadyExists()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var barcodeValue = "1234567890123";
        product.AddBarcode(barcodeValue, true);

        // Act
        var result = product.AddBarcode(barcodeValue, false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RemoveBarcode_ShouldRemoveBarcodeFromProduct()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var barcode = product.AddBarcode("1234567890123", true);

        // Act
        product.RemoveBarcode("1234567890123");

        // Assert
        product.Barcodes.Should().BeEmpty();
    }

    [Fact]
    public void GetPrimaryBarcode_ShouldReturnPrimaryBarcode_WhenExists()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var barcode = product.AddBarcode("1234567890123", true);

        // Act
        var primaryBarcode = product.GetPrimaryBarcode();

        // Assert
        primaryBarcode.Should().NotBeNull();
        primaryBarcode.Should().Be("1234567890123");
    }

    [Fact]
    public void GetPrimaryBarcode_ShouldReturnFirstBarcode_WhenNoPrimaryExists()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);
        var barcode = product.AddBarcode("1234567890123", true);
        barcode.UnsetPrimary();

        // Act
        var primaryBarcode = product.GetPrimaryBarcode();

        // Assert - GetPrimaryBarcode returns first barcode as fallback
        primaryBarcode.Should().Be("1234567890123");
    }

    [Fact]
    public void GetPrimaryBarcode_ShouldReturnNull_WhenNoBarcodesExist()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);

        // Act
        var primaryBarcode = product.GetPrimaryBarcode();

        // Assert
        primaryBarcode.Should().BeNull();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateAllProductProperties()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var brand = new Brand("Dell", "Computer manufacturer");
        var product = new Product("Laptop", category, 1000m, 800m);

        // Act
        product.UpdateDetails(
            name: "Gaming Laptop",
            description: "High-performance gaming laptop",
            price: 1500m,
            costPrice: 1200m,
            minStockLevel: 5,
            maxStockLevel: 50,
            isActive: true,
            productType: "Electronics",
            brand: brand,
            unit: "pcs",
            weight: 2.5m,
            location: "Warehouse A",
            directSalesEnabled: true,
            pointsEnabled: true,
            imageUrl: null
        );

        // Assert
        product.Name.Should().Be("Gaming Laptop");
        product.Description.Should().Be("High-performance gaming laptop");
        product.Price.Should().Be(1500m);
        product.CostPrice.Should().Be(1200m);
        product.MinStockLevel.Should().Be(5);
        product.MaxStockLevel.Should().Be(50);
        product.BrandId.Should().Be(brand.Id);
        product.Unit.Should().Be("pcs");
        product.Weight.Should().Be(2.5m);
        product.Location.Should().Be("Warehouse A");
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        var product = new Product("Laptop", category, 1000m, 800m);

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
    }
}
