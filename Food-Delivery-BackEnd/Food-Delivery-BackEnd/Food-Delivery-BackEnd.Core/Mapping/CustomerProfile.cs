using AutoMapper;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Data.Models;

namespace Food_Delivery_BackEnd.Core.Mapping
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<RegisterUserRequestDto, Customer>();

            CreateMap<Customer, CustomerResponseDto>();

            CreateMap<UpdateUserRequestDto, Customer>();

            CreateMap<Customer, DeleteEntityResponseDto>();
        }
    }
}
