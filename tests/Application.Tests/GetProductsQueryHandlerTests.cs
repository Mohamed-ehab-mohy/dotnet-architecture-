using AutoMapper;
using Application.Common.Mappings;
using Application.Features.Products.DTOs;
using Application.Features.Products.Queries.GetProducts;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Xunit;

namespace Application.Tests;

public class GetProductsQueryHandlerTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IRepository<Product>> _mockRepo;

    public GetProductsQueryHandlerTests()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _mockRepo = new Mock<IRepository<Product>>();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllProducts()
    {
        var products = new List<Product>
        {
            new("Product 1", "Desc 1", 10m),
            new("Product 2", "Desc 2", 20m)
        };

        _mockRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products.AsReadOnly());

        var handler = new GetProductsQueryHandler(_mockRepo.Object, _mapper);
        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Product 1", result[0].Name);
    }
}
