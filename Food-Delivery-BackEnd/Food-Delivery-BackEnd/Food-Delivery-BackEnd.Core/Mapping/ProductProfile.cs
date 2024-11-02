using AutoMapper;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Data.Models;

namespace Food_Delivery_BackEnd.Core.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<CreateProductRequestDto, Product>();

            CreateMap<Product, ProductResponseDto>();

            CreateMap<UpdateProductRequestDto, Product>();

            CreateMap<Product, DeleteEntityResponseDto>();

            CreateMap<Product, ImageResponseDto>();
        }
    }
}
