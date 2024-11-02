using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Core.Services.Interfaces
{
    public interface IAdminService
    {
        public Task<bool> IsEmailTaken(string email);
        public Task<bool> IsUsernameTaken(string username);
        public Task<Admin?> GetAdminById(long id);
        public Task<Admin> RegisterAdmin(Admin admin);
        public Task<Admin> UpdateAdmin(Admin admin);


        public Task<AdminResponseDto> RegisterAdmin(RegisterUserRequestDto requestDto);
        public Task<AdminResponseDto> UpdateAdmin(long id, UpdateUserRequestDto requestDto);

    }
}
