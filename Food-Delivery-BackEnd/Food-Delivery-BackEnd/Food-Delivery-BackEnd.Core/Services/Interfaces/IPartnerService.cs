using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Data.Enums;
using Food_Delivery_BackEnd.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Core.Services.Interfaces
{
    public interface IPartnerService
    {
        public Task<bool> IsEmailTaken(string email);
        public Task<bool> IsUsernameTaken(string username);
        public Task<List<Partner>> GetAllPartners();
        public Task<List<Partner>> GetPartnersByStatus(PartnerStatus status);
        public Task<Partner?> GetPartnerById(long id);
        public Task<Partner> RegisterPartner(Partner partner);
        public Task<Partner> UpdatePartner(Partner partner);
        public Task DeletePartner(Partner partner);




        public Task<List<PartnerResponseDto>> GetPartners(string status);
        public Task<PartnerResponseDto> GetPartner(long id);
        public Task<PartnerResponseDto> RegisterPartners(RegisterUserRequestDto requestDto);
        public Task<PartnerResponseDto> UpdatePartners(long id, UpdateUserRequestDto requestDto);
        public Task<DeleteEntityResponseDto> DeletePartners(long id);
        public Task<PartnerResponseDto> VerifyPartner(long id, VerifyPartnerRequestDto requestDto);

    }
}
