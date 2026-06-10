using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price
) : IRequest<Result<int>>;
