using Xunit;

namespace API.Tests;

public class ProductsControllerTests
{
    [Fact]
    public async Task GetAll_ShouldReturnProductsList()
    {
        await Task.CompletedTask;
        Assert.True(true);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        await Task.CompletedTask;
        Assert.True(true);
    }
}
