using Application.Features.Products.DTOs;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
