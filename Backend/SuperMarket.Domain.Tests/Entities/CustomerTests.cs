using FluentAssertions;
using SuperMarket.Domain.Entities;
using SuperMarket.Domain.ValueObjects;

namespace SuperMarket.Domain.Tests.Entities;

public class CustomerTests
{
    [Fact]
    public void Constructor_ShouldCreateCustomerWithFullData()
    {
        // Arrange
        var name = "John Doe";
        var phone = "123-456-7890";
        var email = "john@example.com";
        var address = new Address("123 Main St", "Downtown", "New York");

        // Act
        var customer = new Customer(name, email, phone, address, DateTime.Parse("1990-01-01"), "Male", "Regular");

        // Assert
        customer.Name.Should().Be(name);
        customer.Phone.Should().Be(phone);
        customer.Email.Should().Be(email);
        customer.Address.Should().Be(address);
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateCustomerWithNullableFields()
    {
        // Arrange & Act
        var customer = new Customer("Jane Doe", null, "987-654-3210", null, null, null, null);

        // Assert
        customer.Name.Should().Be("Jane Doe");
        customer.Phone.Should().Be("987-654-3210");
        customer.Email.Should().BeNull();
        customer.Address.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldUpdateAllProperties()
    {
        // Arrange
        var customer = new Customer("Old Name", null, "111-111-1111", null, null, null, null);
        var newAddress = new Address("456 Oak Ave", "Uptown", "Boston");

        // Act
        customer.Update("New Name", "new@example.com", "222-222-2222", newAddress, DateTime.Parse("1985-05-15"), "Female", "VIP");

        // Assert
        customer.Name.Should().Be("New Name");
        customer.Email.Should().Be("new@example.com");
        customer.Phone.Should().Be("222-222-2222");
        customer.Address.Should().Be(newAddress);
        customer.Gender.Should().Be("Female");
        customer.CustomerType.Should().Be("VIP");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var customer = new Customer("Test", null, "123", null, null, null, null);

        // Act
        customer.Deactivate();

        // Assert
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var customer = new Customer("Test", null, "123", null, null, null, null);
        customer.Deactivate();

        // Act
        customer.Activate();

        // Assert
        customer.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    public void ValidateEmail_ShouldReturnCorrectResult(string? email, bool expected)
    {
        // Act
        var result = Customer.ValidateEmail(email);

        // Assert
        result.Should().Be(expected);
    }
}
