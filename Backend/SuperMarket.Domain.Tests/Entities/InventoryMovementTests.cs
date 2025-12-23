using FluentAssertions;
using SuperMarket.Domain.Entities;

namespace SuperMarket.Domain.Tests.Entities;

public class InventoryMovementTests
{
    [Fact]
    public void Constructor_ShouldCreateInventoryMovementWithValidData()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var type = MovementType.Purchase;
        var quantity = 100;
        var reference = "PUR-2024-001";
        var notes = "Initial stock";

        // Act
        var movement = new InventoryMovement(productId, type, quantity, reference, notes);

        // Assert
        movement.ProductId.Should().Be(productId);
        movement.Type.Should().Be(type);
        movement.Quantity.Should().Be(quantity);
        movement.Reference.Should().Be(reference);
        movement.Notes.Should().Be(notes);
        movement.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldHandleDifferentMovementTypes()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var movementIn = new InventoryMovement(productId, MovementType.Purchase, 50, "REF-IN", "Stock in");
        var movementOut = new InventoryMovement(productId, MovementType.Sale, 30, "REF-OUT", "Stock out");
        var movementAdjustment = new InventoryMovement(productId, MovementType.Adjustment, 10, "REF-ADJ", "Adjustment");
        var movementReturn = new InventoryMovement(productId, MovementType.Return, 5, "REF-RET", "Return");

        // Assert
        movementIn.Type.Should().Be(MovementType.Purchase);
        movementOut.Type.Should().Be(MovementType.Sale);
        movementAdjustment.Type.Should().Be(MovementType.Adjustment);
        movementReturn.Type.Should().Be(MovementType.Return);
    }

    [Fact]
    public void Constructor_ShouldAllowNegativeQuantityForOutMovements()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var movement = new InventoryMovement(productId, MovementType.Sale, -50, "REF-001", "Outbound");

        // Assert
        movement.Quantity.Should().Be(-50);
    }

    [Fact]
    public void Constructor_ShouldAllowNullNotes()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var movement = new InventoryMovement(productId, MovementType.Purchase, 100, "REF-001", null);

        // Assert
        movement.Notes.Should().BeNull();
    }
}
