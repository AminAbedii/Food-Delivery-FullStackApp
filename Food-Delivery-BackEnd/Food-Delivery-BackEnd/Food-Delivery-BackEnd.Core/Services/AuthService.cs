using AutoMapper;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using FluentValidation;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Core.Exceptions;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Food_Delivery_BackEnd.Data.Context;
using Food_Delivery_BackEnd.Data.Enums;
using Food_Delivery_BackEnd.Data.Models;
using Food_Delivery_BackEnd.Core.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfigurationSection _jwtSettings;
        private readonly IConfiguration _configuration; // Change this to IConfiguration  
        private readonly FoodDeliveryDbContext _dbContext;

        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        public AuthService(FoodDeliveryDbContext dbContext, IConfiguration configuration, IMapper mapper, Cloudinary cloudinary)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _mapper = mapper;

            _cloudinary = cloudinary;

            IConfigurationSection cloudinarySettings = _configuration.GetSection("CloudinarySettings");
            string cloudinaryUrl = cloudinarySettings.GetValue<string>("CloudinaryUrl");
            _cloudinary = new Cloudinary(cloudinaryUrl);
        }

        public async Task ChangePassword(long id, UserType userType, ChangePasswordRequestDto requestDto)
        {
            User user = _mapper.Map<User>(requestDto);

            List<string> validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(requestDto.NewPassword))
            {
                validationErrors.Add("New password is required.");
            }
            else if (requestDto.NewPassword.Length < 6) 
            {
                validationErrors.Add("New password must be at least 6 characters long.");
            }

            if (string.IsNullOrWhiteSpace(requestDto.OldPassword))
            {
                validationErrors.Add("Old password is required.");
            }

            if (validationErrors.Any())
            {
                throw new ValidationException(string.Join(", ", validationErrors));
            }

            User? existingUser = await GetUserById(id, userType);

            if (existingUser == null)
            {
                throw new ResourceNotFoundException("User with this id doesn't exist");
            }

            if (!BCrypt.Net.BCrypt.Verify(requestDto.OldPassword, existingUser.Password))
            {
                throw new IncorrectLoginCredentialsException("Incorrect password");
            }

            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(requestDto.NewPassword, salt);

            existingUser = await UpdateUser(existingUser);

            return;
        }

        public async Task<RefreshToken> CreateRefreshToken(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
            return refreshToken;
        }

        public async Task DeleteRefreshToken(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Remove(refreshToken);
            await _dbContext.SaveChangesAsync();
            return;
        }

        public async Task DeleteToken(long userId, UserType userType, DeleteTokenRequestDto requestDto)
        {
            User? user = await GetUserById(userId, userType);

            if (user == null)
            {
                throw new ResourceNotFoundException("User with this id doesn't exist");
            }

            string refreshToken = requestDto.RefreshToken;

            RefreshToken? existingRefreshToken = await GetRefreshToken(refreshToken);

            if (existingRefreshToken == null)
            {
                throw new IncorrectLoginCredentialsException("Provided refresh token is not valid");
            }

            await DeleteRefreshToken(existingRefreshToken);

            return;
        }

        public async Task<TokenResponseDto> GenerateToken(CreateTokenRequestDto requestDto)
        {
            GrantType grantType = requestDto.GrantType;

            if (grantType == GrantType.UsernamePassword)
            {
                UserType? userType = requestDto.UserType;
                string? username = requestDto.Username;
                string? password = requestDto.Password;

                if (userType == null)
                {
                    throw new IncorrectLoginCredentialsException("User type is required");
                }

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new IncorrectLoginCredentialsException("Username and password are required");
                }

                User? user = await GetUserByUsername(username, userType.Value);

                if (user == null)
                {
                    throw new IncorrectLoginCredentialsException("Incorrect username");
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    throw new IncorrectLoginCredentialsException("Incorrect password");
                }

                List<Claim> claims = new List<Claim>()
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, userType.Value.ToString())
                };

                if (user is Partner partner)
                {
                    claims.Add(new Claim("Status", partner.Status.ToString()));
                }

                string jwtSecretKey = _jwtSettings.GetValue<string>("SecretKey");
                string jwtIssuer = _jwtSettings.GetValue<string>("ValidIssuer");

                int accessTokenExpiresIn = 1800;
                int refreshTokenExpiresIn = 2592000;

                string accessTokenPayload = TokenHelpers.GenerateAccessToken(jwtSecretKey, jwtIssuer, claims, accessTokenExpiresIn);
                string refreshTokenPayload = TokenHelpers.GenerateRefreshToken();

                TokenResponseDto responseDto = new TokenResponseDto()
                {
                    AccessToken = accessTokenPayload,
                    ExpiresIn = accessTokenExpiresIn,
                    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                RefreshToken? existingRefreshToken = await GetRefreshTokenByUser(user.Id);

                if (existingRefreshToken != null)
                {
                    existingRefreshToken.Token = refreshTokenPayload;
                    existingRefreshToken.CreatedAt = DateTime.UtcNow;

                    existingRefreshToken = await UpdateRefreshToken(existingRefreshToken);

                    responseDto.RefreshToken = existingRefreshToken.Token;

                    return responseDto;
                }

                RefreshToken refreshToken = new RefreshToken()
                {
                    Token = refreshTokenPayload,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresIn = refreshTokenExpiresIn,
                    UserId = user.Id,
                    UserType = userType.Value
                };

                refreshToken = await CreateRefreshToken(refreshToken);

                responseDto.RefreshToken = refreshToken.Token;

                return responseDto;
            }
            else if (grantType == GrantType.RefreshToken)
            {
                string? refreshToken = requestDto.RefreshToken;

                if (string.IsNullOrEmpty(refreshToken))
                {
                    throw new IncorrectLoginCredentialsException("Refresh token is required");
                }

                RefreshToken? existingRefreshToken = await GetRefreshToken(refreshToken);

                if (existingRefreshToken == null)
                {
                    throw new IncorrectLoginCredentialsException("Provided refresh token is not valid");
                }

                User? user = await GetUserById(existingRefreshToken.UserId, existingRefreshToken.UserType);

                if (user == null)
                {
                    throw new ResourceNotFoundException("User with this id doesn't exist");
                }

                List<Claim> claims = new List<Claim>()
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, existingRefreshToken.UserType.ToString())
                };

                if (user is Partner partner)
                {
                    claims.Add(new Claim("Status", partner.Status.ToString()));
                }

                string jwtSecretKey = _jwtSettings.GetValue<string>("SecretKey");
                string jwtIssuer = _jwtSettings.GetValue<string>("ValidIssuer");

                int accessTokenExpiresIn = 1800;

                string accessToken = TokenHelpers.GenerateAccessToken(jwtSecretKey, jwtIssuer, claims, accessTokenExpiresIn);

                TokenResponseDto responseDto = new TokenResponseDto()
                {
                    AccessToken = accessToken,
                    RefreshToken = existingRefreshToken.Token,
                    ExpiresIn = accessTokenExpiresIn,
                    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                return responseDto;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public async Task<UserResponseDto> GetProfile(long userId, UserType userType)
        {
            User? existingUser = await GetUserById(userId, userType);

            if (existingUser == null)
            {
                throw new ResourceNotFoundException("User with this id doesn't exist");
            }

            UserResponseDto responseDto = new UserResponseDto();

            switch (userType)
            {
                case UserType.Customer:
                    responseDto = _mapper.Map<CustomerResponseDto>(existingUser);
                    break;
                case UserType.Partner:
                    responseDto = _mapper.Map<PartnerResponseDto>(existingUser);
                    break;
                case UserType.Admin:
                    responseDto = _mapper.Map<AdminResponseDto>(existingUser);
                    break;
                default:
                    responseDto = _mapper.Map<UserResponseDto>(existingUser);
                    break;
            }

            responseDto.UserType = userType;

            return responseDto;
        }

        public async Task<RefreshToken?> GetRefreshToken(string token)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<RefreshToken?> GetRefreshTokenByUser(long userId)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<User?> GetUserById(long id, UserType type)
        {
            switch (type)
            {
                case UserType.Customer:
                    return await _dbContext.Customers.FindAsync(id);
                case UserType.Partner:
                    return await _dbContext.Partners.FindAsync(id);
                case UserType.Admin:
                    return await _dbContext.Admins.FindAsync(id);
                default:
                    throw new ArgumentException("Invalid user type");
            }
        }

        public async Task<User?> GetUserByUsername(string username, UserType userType)
        {
            switch (userType)
            {
                case UserType.Customer:
                    return await _dbContext.Customers.Where(x => x.Username == username).FirstOrDefaultAsync();
                case UserType.Partner:
                    return await _dbContext.Partners.Where(x => x.Username == username).FirstOrDefaultAsync();
                case UserType.Admin:
                    return await _dbContext.Admins.Where(x => x.Username == username).FirstOrDefaultAsync();
                default:
                    throw new ArgumentException("Invalid user type");
            }
        }

        public async Task RemoveImage(long userId, UserType userType)
        {
            User? existingUser = await GetUserById(userId, userType);

            if (existingUser == null)
            {
                throw new ResourceNotFoundException("User with this id doesn't exist");
            }

            if (existingUser.ImagePublicId != null)
            {
                DeletionParams deletionParams = new DeletionParams(existingUser.ImagePublicId)
                {
                    ResourceType = ResourceType.Image
                };

                await _cloudinary.DestroyAsync(deletionParams);
            }

            existingUser.ImagePublicId = null;
            existingUser.Image = null;

            await UpdateUser(existingUser);

            return;
        }

        public async Task<UserResponseDto> UpdateProfile(long userId, UserType userType, UpdateUserRequestDto requestDto)
        {
            User user = _mapper.Map<User>(requestDto);

            List<string> validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                validationErrors.Add("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Email)) /*|| !IsValidEmail(user.Email))*/
            {
                validationErrors.Add("A valid email is required.");
            }

            if (string.IsNullOrWhiteSpace(user.FirstName))
            {
                validationErrors.Add("First name is required.");
            }

            if (string.IsNullOrWhiteSpace(user.LastName))
            {
                validationErrors.Add("Last name is required.");
            }

            if (validationErrors.Any())
            {
                throw new ValidationException(string.Join(", ", validationErrors));
            }

            User? existingUser = await GetUserById(userId, userType);

            if (existingUser == null)
            {
                throw new ResourceNotFoundException("User with this id doesn't exist");
            }

            _mapper.Map(requestDto, existingUser);

            existingUser = await UpdateUser(existingUser);

            UserResponseDto responseDto = new UserResponseDto();

            switch (userType)
            {
                case UserType.Customer:
                    responseDto = _mapper.Map<CustomerResponseDto>(existingUser);
                    break;
                case UserType.Partner:
                    responseDto = _mapper.Map<PartnerResponseDto>(existingUser);
                    break;
                case UserType.Admin:
                    responseDto = _mapper.Map<AdminResponseDto>(existingUser);
                    break;
                default:
                    responseDto = _mapper.Map<UserResponseDto>(existingUser);
                    break;
            }

            responseDto.UserType = userType;

            return responseDto;
        }

        public async Task<RefreshToken> UpdateRefreshToken(RefreshToken refreshToken)
        {
            _dbContext.Entry(refreshToken).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<User> UpdateUser(User user)
        {
            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<ImageResponseDto> UploadImage(long id, UserType userType, Stream imageStream, string imageName)
        {
            if (imageStream == null || imageStream.Length == 0)
            {
                throw new InvalidImageException("Provided image is invalid. Please ensure that the image has valid content");
            }

            User? existingUser = await GetUserById(id, userType);

            if (existingUser == null)
            {
                throw new ResourceNotFoundException("User with this id doesn't exist");
            }

            if (existingUser.ImagePublicId != null)
            {
                DeletionParams deletionParams = new DeletionParams(existingUser.ImagePublicId)
                {
                    ResourceType = ResourceType.Image
                };

                await _cloudinary.DestroyAsync(deletionParams);
            }

            ImageUploadParams uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(imageName, imageStream),
                Tags = "users"
            };

            ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams);

            existingUser.ImagePublicId = uploadResult.PublicId;
            existingUser.Image = uploadResult.Url.ToString();

            existingUser = await UpdateUser(existingUser);

            return _mapper.Map<ImageResponseDto>(existingUser);
        }
    }
}
