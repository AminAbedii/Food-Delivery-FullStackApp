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
    public class CustomerService : ICustomerService
    {

        private readonly FoodDeliveryDbContext _dbContext;
        private readonly IMapper _mapper;
        public CustomerService(FoodDeliveryDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            return await _dbContext.Customers.ToListAsync();
        }

        public async Task<Customer?> GetCustomerById(long id)
        {
            return await _dbContext.Customers.FindAsync(id);
        }

        public async Task<bool> IsEmailTaken(string email)
        {
            return await _dbContext.Customers.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> IsUsernameTaken(string username)
        {
            return await _dbContext.Customers.AnyAsync(x => x.Username == username);
        }

        public async Task<Customer> RegisterCustomer(Customer customer)
        {
            await _dbContext.Customers.AddAsync(customer);
            await _dbContext.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateCustomer(Customer customer)
        {
            _dbContext.Entry(customer).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return customer;
        }

        public async Task DeleteCustomer(Customer customer)
        {
            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();
            return;
        }

        public async Task<List<CustomerResponseDto>> GetCustomers()
        {
            List<Customer> customers = await GetAllCustomers();

            return _mapper.Map<List<CustomerResponseDto>>(customers);
        }

        public async Task<CustomerResponseDto> GetCustomer(long id)
        {
            Customer? customer = await GetCustomerById(id);

            if (customer == null)
            {
                throw new ResourceNotFoundException("Customer with this id doesn't exist");
            }

            return _mapper.Map<CustomerResponseDto>(customer);
        }

        public async Task<CustomerResponseDto> RegisterCustomers(RegisterUserRequestDto requestDto)
        {
            Customer customer = _mapper.Map<Customer>(requestDto);

            if (string.IsNullOrWhiteSpace(customer.Email))
            {
                throw new ValidationException("Email cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(customer.Username))
            {
                throw new ValidationException("Username cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(customer.Password) || customer.Password.Length < 6)
            {
                throw new ValidationException("Password must be at least 6 characters long.");
            }

            // Check for existing email and username
            if (await IsEmailTaken(customer.Email))
            {
                throw new UserAlreadyExistsException("Customer with this email already exists");
            }

            if (await IsUsernameTaken(customer.Username))
            {
                throw new UserAlreadyExistsException("Customer with this username already exists");
            }
            //new UserAlreadyExistsException("Customer with this username already exists");
            //}

            // Hash password
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            customer.Password = BCrypt.Net.BCrypt.HashPassword(customer.Password, salt);

            customer = await RegisterCustomer(customer);

            CustomerResponseDto responseDto = _mapper.Map<CustomerResponseDto>(customer);
            responseDto.UserType = UserType.Customer;

            return responseDto;
        }

        public async Task<CustomerResponseDto> UpdateCustomers(long id, UpdateUserRequestDto requestDto)
        {
            Customer? customer = await GetCustomerById(id);

            if (customer == null)
            {
                throw new ResourceNotFoundException("Customer with this id doesn't exist");
            }

            // Map the request DTO to a new customer object for validation
            Customer updatedCustomer = _mapper.Map<Customer>(requestDto);

            // Custom validation logic
            if (string.IsNullOrWhiteSpace(updatedCustomer.Username))
            {
                throw new ValidationException("Username cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedCustomer.Email))
            {
                throw new ValidationException("Email cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedCustomer.FirstName))
            {
                throw new ValidationException("First name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedCustomer.LastName))
            {
                throw new ValidationException("Last name cannot be empty.");
            }

            // Check for existing email and username, ensuring they are not the same as the current values
            if (await IsEmailTaken(updatedCustomer.Email) && updatedCustomer.Email != customer.Email)
            {
                throw new UserAlreadyExistsException("Customer with this email already exists");
            }

            if (await IsUsernameTaken(updatedCustomer.Username) && updatedCustomer.Username != customer.Username)
            {
                throw new UserAlreadyExistsException("Customer with this username already exists");
            }

            // Map the updated values back to the original customer object
            _mapper.Map(requestDto, customer);

            customer = await UpdateCustomer(customer);

            CustomerResponseDto responseDto = _mapper.Map<CustomerResponseDto>(customer);
            responseDto.UserType = UserType.Customer;

            return responseDto;
        }

        public async Task<DeleteEntityResponseDto> DeleteCustomers(long id)
        {
            Customer? customer = await GetCustomerById(id);

            if (customer == null)
            {
                throw new ResourceNotFoundException("Customer with this id doesn't exist");
            }

            await DeleteCustomer(customer);

            return _mapper.Map<DeleteEntityResponseDto>(customer);
        }
    }
}
