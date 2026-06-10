using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    int Id,
    string Name,
    string Description,
    decimal Price
) : IRequest<Result<int>>;
