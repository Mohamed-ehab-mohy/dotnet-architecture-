using Application.Common.Models;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<int>>
{
    private readonly IRepository<Product> _repository;

    public CreateProductCommandHandler(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<Result<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Name, request.Description, request.Price);
        await _repository.AddAsync(product);
        return Result<int>.Success(product.Id);
    }
}
