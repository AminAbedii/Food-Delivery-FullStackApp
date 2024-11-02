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
    public interface ICustomerService
    {
        public Task<bool> IsEmailTaken(string email);
        public Task<bool> IsUsernameTaken(string username);
        public Task<List<Customer>> GetAllCustomers();
        public Task<Customer?> GetCustomerById(long id);
        public Task<Customer> RegisterCustomer(Customer customer);
        public Task<Customer> UpdateCustomer(Customer customer);
        public Task DeleteCustomer(Customer customer);



        public Task<List<CustomerResponseDto>> GetCustomers();
        public Task<CustomerResponseDto> GetCustomer(long id);
        public Task<CustomerResponseDto> RegisterCustomers(RegisterUserRequestDto requestDto);
        public Task<CustomerResponseDto> UpdateCustomers(long id, UpdateUserRequestDto requestDto);
        public Task<DeleteEntityResponseDto> DeleteCustomers(long id);
    }
}
