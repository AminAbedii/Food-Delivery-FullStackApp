using AutoMapper;
using FluentValidation;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Core.Services
{
    public class AdminService : IAdminService
    {
        private readonly FoodDeliveryDbContext _dbContext;
        //private readonly IValidator<Admin> _validator;
        private readonly IMapper _mapper;

        public AdminService(FoodDeliveryDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }


        public async Task<bool> IsEmailTaken(string email)
        {
            return await _dbContext.Admins.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> IsUsernameTaken(string username)
        {
            return await _dbContext.Admins.AnyAsync(x => x.Username == username);
        }

        public async Task<Admin?> GetAdminById(long id)
        {
            return await _dbContext.Admins.FindAsync(id);
        }

        public async Task<Admin> RegisterAdmin(Admin admin)
        {
            await _dbContext.Admins.AddAsync(admin);
            await _dbContext.SaveChangesAsync();
            return admin;
        }

        public async Task<Admin> UpdateAdmin(Admin admin)
        {
            _dbContext.Entry(admin).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return admin;
        }

        public async Task<AdminResponseDto> RegisterAdmin(RegisterUserRequestDto requestDto)
        {
            Admin admin = _mapper.Map<Admin>(requestDto);

            // Custom checks instead of validation
            bool hasErrors = false;
            List<string> errors = new List<string>();

            // Check for empty username or email
            if (string.IsNullOrEmpty(admin.Username))
            {
                hasErrors = true;
                errors.Add("Username cannot be empty.");
            }

            if (string.IsNullOrEmpty(admin.Email))
            {
                hasErrors = true;
                errors.Add("Email cannot be empty.");
            }

            // Check if email is taken
            if (await IsEmailTaken(admin.Email))
            {
                hasErrors = true;
                errors.Add("Admin with this email already exists.");
            }

            // Check if username is taken
            if (await IsUsernameTaken(admin.Username))
            {
                hasErrors = true;
                errors.Add("Admin with this username already exists.");
            }

            // Throw exception if there are any errors
            if (hasErrors)
            {
                throw new ValidationException(string.Join(", ", errors));
            }

            // Hash password
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password, salt);

            admin = await RegisterAdmin(admin);

            AdminResponseDto responseDto = _mapper.Map<AdminResponseDto>(admin);
            responseDto.UserType = UserType.Admin;

            return responseDto;
        }

        public async Task<AdminResponseDto> UpdateAdmin(long id, UpdateUserRequestDto requestDto)
        {
            Admin? admin = await GetAdminById(id);

            if (admin == null)
            {
                throw new ResourceNotFoundException("Admin with this id doesn't exist");
            }

            Admin updatedAdmin = _mapper.Map<Admin>(requestDto);

            // Custom validation logic
            List<string> errors = new List<string>();

            // Check for empty fields
            if (string.IsNullOrEmpty(updatedAdmin.Username))
            {
                errors.Add("Username cannot be empty.");
            }

            if (string.IsNullOrEmpty(updatedAdmin.Email))
            {
                errors.Add("Email cannot be empty.");
            }

            if (string.IsNullOrEmpty(updatedAdmin.FirstName))
            {
                errors.Add("First name cannot be empty.");
            }

            if (string.IsNullOrEmpty(updatedAdmin.LastName))
            {
                errors.Add("Last name cannot be empty.");
            }

            // Check if email is taken (but not the same as current admin's email)
            if (await IsEmailTaken(updatedAdmin.Email) && updatedAdmin.Email != admin.Email)
            {
                errors.Add("Admin with this email already exists.");
            }

            // Check if username is taken (but not the same as current admin's username)
            if (await IsUsernameTaken(updatedAdmin.Username) && updatedAdmin.Username != admin.Username)
            {
                errors.Add("Admin with this username already exists.");
            }

            // Throw exception if there are any validation errors
            if (errors.Count > 0)
            {
                throw new ValidationException(string.Join(", ", errors)); // Join errors into a single string
            }

            _mapper.Map(requestDto, admin);

            admin = await UpdateAdmin(admin);

            AdminResponseDto responseDto = _mapper.Map<AdminResponseDto>(admin);
            responseDto.UserType = UserType.Admin;

            return responseDto;
        }
    }
}
