using AutoMapper;
using Application.Features.Products.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync();
        return _mapper.Map<IReadOnlyList<ProductDto>>(products);
    }
}
