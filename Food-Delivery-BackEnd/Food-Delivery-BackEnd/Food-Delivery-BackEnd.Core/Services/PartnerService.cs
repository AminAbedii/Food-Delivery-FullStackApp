using AutoMapper;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Core.Exceptions;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Food_Delivery_BackEnd.Data.Context;
using Food_Delivery_BackEnd.Data.Enums;
using Food_Delivery_BackEnd.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Core.Services
{
    public class PartnerService : IPartnerService
    {
        private readonly FoodDeliveryDbContext _dbContext;
        private readonly IMapper _mapper;

        public PartnerService(FoodDeliveryDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<Partner>> GetAllPartners()
        {
            return await _dbContext.Partners.ToListAsync();
        }

        public async Task<Partner?> GetPartnerById(long id)
        {
            return await _dbContext.Partners.FindAsync(id);
        }

        public async Task<List<Partner>> GetPartnersByStatus(PartnerStatus status)
        {
            return await _dbContext.Partners.Where(x => x.Status == status).ToListAsync();
        }

        public async Task<bool> IsEmailTaken(string email)
        {
            return await _dbContext.Partners.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> IsUsernameTaken(string username)
        {
            return await _dbContext.Partners.AnyAsync(x => x.Username == username);
        }

        public async Task<Partner> RegisterPartner(Partner partner)
        {
            await _dbContext.Partners.AddAsync(partner);
            await _dbContext.SaveChangesAsync();
            return partner;
        }

        public async Task<Partner> UpdatePartner(Partner partner)
        {
            _dbContext.Entry(partner).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return partner;
        }

        public async Task DeletePartner(Partner partner)
        {
            _dbContext.Partners.Remove(partner);
            await _dbContext.SaveChangesAsync();
            return;
        }

        public async Task<List<PartnerResponseDto>> GetPartners(string status)
        {
            List<Partner> partners = new List<Partner>();

            if (string.IsNullOrEmpty(status))
            {
                partners = await GetAllPartners();
            }
            else
            {
                PartnerStatus partnerStatus;

                if (!Enum.TryParse(status, out partnerStatus))
                {
                    throw new ArgumentException("Invalid status. Status must be one of the following: " + string.Join(", ", Enum.GetNames(typeof(PartnerStatus))));
                }

                partners = await GetPartnersByStatus(partnerStatus);
            }

            return _mapper.Map<List<PartnerResponseDto>>(partners);
        }

        public async Task<PartnerResponseDto> GetPartner(long id)
        {
            Partner? partner = await GetPartnerById(id);

            if (partner == null)
            {
                throw new ResourceNotFoundException("Partner with this id doesn't exist");
            }

            return _mapper.Map<PartnerResponseDto>(partner);
        }

        public async Task<PartnerResponseDto> RegisterPartners(RegisterUserRequestDto requestDto)
        {
            Partner partner = _mapper.Map<Partner>(requestDto);
            partner.Status = PartnerStatus.Pending;

            if (string.IsNullOrWhiteSpace(partner.Email))
            {
                throw new ValidationException("Invalid email address.");
            }

            if (string.IsNullOrWhiteSpace(partner.Username))
            {
                throw new ValidationException("Username cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(partner.Password) || partner.Password.Length < 6)
            {
                throw new ValidationException("Password must be at least 6 characters long.");
            }

            // Check if email is already taken
            if (await IsEmailTaken(partner.Email))
            {
                throw new UserAlreadyExistsException("Partner with this email already exists");
            }

            // Check if username is already taken
            if (await IsUsernameTaken(partner.Username))
            {
                throw new UserAlreadyExistsException("Partner with this username already exists");
            }

            // Hash password
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            partner.Password = BCrypt.Net.BCrypt.HashPassword(partner.Password, salt);

            partner = await RegisterPartner(partner);

            PartnerResponseDto responseDto = _mapper.Map<PartnerResponseDto>(partner);
            responseDto.UserType = UserType.Partner;

            return responseDto;
        }

        public async Task<PartnerResponseDto> UpdatePartners(long id, UpdateUserRequestDto requestDto)
        {
            Partner? partner = await GetPartnerById(id);

            if (partner == null)
            {
                throw new ResourceNotFoundException("Partner with this id doesn't exist");
            }

            Partner updatedPartner = _mapper.Map<Partner>(requestDto);

            if (string.IsNullOrWhiteSpace(updatedPartner.Username))
            {
                throw new ValidationException("Username cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedPartner.Email))
            {
                throw new ValidationException("Invalid email address.");
            }

            if (string.IsNullOrWhiteSpace(updatedPartner.FirstName))
            {
                throw new ValidationException("First name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedPartner.LastName))
            {
                throw new ValidationException("Last name cannot be empty.");
            }

            if (await IsEmailTaken(updatedPartner.Email) && updatedPartner.Email != partner.Email)
            {
                throw new UserAlreadyExistsException("Partner with this email already exists");
            }

            if (await IsUsernameTaken(updatedPartner.Username) && updatedPartner.Username != partner.Username)
            {
                throw new UserAlreadyExistsException("Partner with this username already exists");
            }

            _mapper.Map(requestDto, partner);

            partner = await UpdatePartner(partner);

            PartnerResponseDto responseDto = _mapper.Map<PartnerResponseDto>(partner);
            responseDto.UserType = UserType.Partner;

            return responseDto;
        }

        public async Task<DeleteEntityResponseDto> DeletePartners(long id)
        {
            Partner? partner = await GetPartnerById(id);

            if (partner == null)
            {
                throw new ResourceNotFoundException("Partner with this id doesn't exist");
            }

            await DeletePartner(partner);

            return _mapper.Map<DeleteEntityResponseDto>(partner);
        }

        public async Task<PartnerResponseDto> VerifyPartner(long id, VerifyPartnerRequestDto requestDto)
        {
            Partner? partner = await GetPartnerById(id);

            if (partner == null)
            {
                throw new ResourceNotFoundException("Partner with this id doesn't exist");
            }

            _mapper.Map(requestDto, partner);

            partner = await UpdatePartner(partner);

            return _mapper.Map<PartnerResponseDto>(partner);
        }
    }
}
