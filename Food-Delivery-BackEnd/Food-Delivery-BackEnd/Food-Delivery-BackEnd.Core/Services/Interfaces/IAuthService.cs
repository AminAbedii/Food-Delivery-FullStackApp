using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Data.Enums;
using Food_Delivery_BackEnd.Data.Models;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Core.Services.Interfaces
{
    public interface IAuthService
    {
        //Repository
        public Task<User?> GetUserByUsername(string username, UserType userType);
        public Task<User?> GetUserById(long id, UserType type);
        public Task<User> UpdateUser(User user);
        public Task<RefreshToken> CreateRefreshToken(RefreshToken refreshToken);
        public Task<RefreshToken?> GetRefreshToken(string token);
        public Task<RefreshToken?> GetRefreshTokenByUser(long userId);
        public Task DeleteRefreshToken(RefreshToken refreshToken);
        public Task<RefreshToken> UpdateRefreshToken(RefreshToken refreshToken);




        public Task<UserResponseDto> GetProfile(long userId, UserType userType);
        public Task<TokenResponseDto> GenerateToken(CreateTokenRequestDto requestDto);
        public Task DeleteToken(long userId, UserType userType, DeleteTokenRequestDto requestDto);
        public Task<UserResponseDto> UpdateProfile(long userId, UserType userType, UpdateUserRequestDto requestDto);
        public Task ChangePassword(long id, UserType userType, ChangePasswordRequestDto requestDto);
        public Task<ImageResponseDto> UploadImage(long id, UserType userType, Stream imageStream, string imageName);
        public Task RemoveImage(long userId, UserType userType);
    }
}
