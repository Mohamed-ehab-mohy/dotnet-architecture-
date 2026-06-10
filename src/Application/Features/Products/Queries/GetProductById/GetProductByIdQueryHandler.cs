using AutoMapper;
using Application.Features.Products.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }
}
