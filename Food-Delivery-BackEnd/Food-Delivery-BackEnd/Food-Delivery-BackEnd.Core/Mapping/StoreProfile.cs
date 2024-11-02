using AutoMapper;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Data.Models;

namespace Food_Delivery_BackEnd.Core.Mapping
{
    public class StoreProfile : Profile
    {
        public StoreProfile()
        {
            CreateMap<StoreRequestDto, Store>();

            //CreateMap<Store, StoreResponseDto>()
            //    .ForMember(dest => dest.Coordinates, opt => opt.MapFrom(src => src.DeliveryArea.Coordinates));

            CreateMap<Store, DeleteEntityResponseDto>();

            CreateMap<Store, ImageResponseDto>();
        }
    }
}
