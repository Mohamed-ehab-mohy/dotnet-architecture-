using Application.Features.Products.Commands.CreateProduct;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Xunit;

namespace Application.Tests;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnProductId()
    {
        var mockRepo = new Mock<IRepository<Product>>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => p.GetType().GetProperty("Id")?.SetValue(p, 1))
            .Returns(Task.CompletedTask);

        var handler = new CreateProductCommandHandler(mockRepo.Object);
        var command = new CreateProductCommand("Test", "Desc", 99.99m);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
    }
}
