using FluentAssertions;
using SuperMarket.Domain.Entities;
using SuperMarket.Domain.ValueObjects;

namespace SuperMarket.Domain.Tests.Entities;

public class ProviderTests
{
    [Fact]
    public void Constructor_ShouldCreateProviderWithValidData()
    {
        // Arrange
        var name = "Tech Supplier Inc.";
        var contactPerson = "Alice Smith";
        var phone = "555-1234";
        var email = "contact@techsupplier.com";
        var address = new Address("789 Industrial Blvd", "Tech Park", "San Francisco");

        // Act
        var provider = new Provider(name, Provider.GenerateCode(name), phone, email, address, null, "Tech Supplier Corp", "TAX123456");

        // Assert
        provider.Name.Should().Be(name);
        provider.Phone.Should().Be(phone);
        provider.Email.Should().Be(email);
        provider.Address.Should().Be(address);
        provider.Code.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void UpdateProvider_ShouldUpdateAllProperties()
    {
        // Arrange
        var provider = new Provider("Old Name", "CODE001", "111", "old@example.com", null, null, null, null);
        var newAddress = new Address("New Street", "New District", "New City");

        // Act
        provider.UpdateProvider("New Name", "CODE002", "222", "new@example.com", newAddress, "Updated note", "New Company", "TAX999");

        // Assert
        provider.Name.Should().Be("New Name");
        provider.Code.Should().Be("CODE002");
        provider.Phone.Should().Be("222");
        provider.Email.Should().Be("new@example.com");
        provider.Address.Should().Be(newAddress);
    }

    [Fact]
    public void GenerateCode_ShouldGenerateValidFormat()
    {
        // Act
        var code = Provider.GenerateCode("Test Provider");

        // Assert
        code.Should().StartWith("TE");
        code.Should().HaveLength(6);
    }

    [Fact]
    public void GenerateCode_ShouldGenerateUniqueCodesInSequence()
    {
        // Act
        var code1 = Provider.GenerateCode("Provider One");
        var code2 = Provider.GenerateCode("Provider Two");

        // Assert
        code1.Should().NotBe(code2);
    }

    [Fact]
    public void GenerateCode_ShouldHandleSpecialCharacters()
    {
        // Act
        var code = Provider.GenerateCode("Test & Provider!");

        // Assert
        code.Should().MatchRegex("^SUP[A-Z0-9]+$");
    }

    [Fact]
    public void GenerateCode_ShouldNotReturnEmptyString()
    {
        // Act
        var code = Provider.GenerateCode("Test");

        // Assert
        code.Should().NotBeEmpty();
    }

    [Fact]
    public void Purchases_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var provider = new Provider("Test", "CODE", "123", "test@example.com", null, null, null, null);

        // Assert
        provider.Purchases.Should().NotBeNull();
        provider.Purchases.Should().BeEmpty();
    }
}
