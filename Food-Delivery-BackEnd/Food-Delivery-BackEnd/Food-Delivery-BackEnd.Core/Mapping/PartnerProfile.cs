using AutoMapper;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Data.Models;

namespace Food_Delivery_BackEnd.Core.Mapping
{
    public class PartnerProfile : Profile
    {
        public PartnerProfile()
        {
            CreateMap<RegisterUserRequestDto, Partner>();

            CreateMap<Partner, PartnerResponseDto>();

            CreateMap<UpdateUserRequestDto, Partner>();

            CreateMap<Partner, DeleteEntityResponseDto>();

            CreateMap<VerifyPartnerRequestDto, Partner>();
        }
    }
}
