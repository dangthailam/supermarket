using FluentAssertions;
using SuperMarket.Domain.Entities;

namespace SuperMarket.Domain.Tests.Entities;

public class CategoryAndBrandTests
{
    [Fact]
    public void Category_ShouldBeCreatedWithObjectInitializer()
    {
        // Act
        var category = new Category { Name = "Electronics", Description = "Electronic devices" };

        // Assert
        category.Name.Should().Be("Electronics");
        category.Description.Should().Be("Electronic devices");
    }

    [Fact]
    public void Brand_ShouldBeCreatedWithConstructor()
    {
        // Act
        var brand = new Brand("Dell", "Computer manufacturer");

        // Assert
        brand.Name.Should().Be("Dell");
        brand.Description.Should().Be("Computer manufacturer");
    }
}
