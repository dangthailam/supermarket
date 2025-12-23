using FluentAssertions;
using SuperMarket.Domain.Entities;

namespace SuperMarket.Domain.Tests.Entities;

public class ProductBarcodeTests
{
    [Fact]
    public void Constructor_ShouldCreateBarcodeWithPrimaryFlag()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var barcodeValue = "1234567890123";
        var isPrimary = true;

        // Act
        var barcode = new ProductBarcode(productId, barcodeValue, isPrimary);

        // Assert
        barcode.ProductId.Should().Be(productId);
        barcode.Barcode.Should().Be(barcodeValue);
        barcode.IsPrimary.Should().Be(isPrimary);
    }

    [Fact]
    public void UnsetPrimary_ShouldSetIsPrimaryToFalse()
    {
        // Arrange
        var barcode = new ProductBarcode(Guid.NewGuid(), "1234567890123", true);

        // Act
        barcode.UnsetPrimary();

        // Assert
        barcode.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void SetAsPrimary_ShouldSetIsPrimaryToTrue()
    {
        // Arrange
        var barcode = new ProductBarcode(Guid.NewGuid(), "1234567890123", false);

        // Act
        barcode.SetAsPrimary();

        // Assert
        barcode.IsPrimary.Should().BeTrue();
    }
}
