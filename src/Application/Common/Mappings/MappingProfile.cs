using AutoMapper;
using Domain.Entities;
using Application.Features.Products.DTOs;

namespace Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
    }
}
