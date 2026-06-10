using Application.Features.Products.DTOs;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
