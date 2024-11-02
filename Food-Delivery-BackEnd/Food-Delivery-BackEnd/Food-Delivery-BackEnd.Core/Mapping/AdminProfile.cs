using AutoMapper;
using Food_Delivery_BackEnd.Data.Models;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;

namespace Food_Delivery_BackEnd.Core.Mapping
{
    public class AdminProfile : Profile
    {
        public AdminProfile()
        {
            CreateMap<RegisterUserRequestDto, Admin>();

            CreateMap<Admin, AdminResponseDto>();

            CreateMap<UpdateUserRequestDto, Admin>();
        }
    }
}
