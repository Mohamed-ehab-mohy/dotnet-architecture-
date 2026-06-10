using Application.Common.Models;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<int>>
{
    private readonly IRepository<Product> _repository;

    public UpdateProductCommandHandler(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<Result<int>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);
        if (product is null)
            return Result<int>.Failure($"Product with ID {request.Id} was not found.");

        product.UpdateDetails(request.Name, request.Description, request.Price);
        await _repository.UpdateAsync(product);

        return Result<int>.Success(product.Id);
    }
}
