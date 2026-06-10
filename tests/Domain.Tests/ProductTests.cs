using Domain.Entities;
using Xunit;

namespace Domain.Tests;

public class ProductTests
{
    [Fact]
    public void CreateProduct_WithValidData_ShouldSucceed()
    {
        var product = new Product("Test Product", "Description", 99.99m);

        Assert.Equal("Test Product", product.Name);
        Assert.Equal("Description", product.Description);
        Assert.Equal(99.99m, product.Price);
    }

    [Fact]
    public void CreateProduct_WithZeroPrice_ShouldThrowException()
    {
        Assert.Throws<Exceptions.DomainException>(() =>
            new Product("Test", "Desc", 0));
    }

    [Fact]
    public void UpdateProductDetails_ShouldUpdateProperties()
    {
        var product = new Product("Old", "Old Desc", 10m);
        product.UpdateDetails("New", "New Desc", 20m);

        Assert.Equal("New", product.Name);
        Assert.Equal(20m, product.Price);
    }
}
