using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Food_Delivery_BackEnd.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCustomers()
        {
            List<CustomerResponseDto> responseDto = await _customerService.GetCustomers();

            return Ok(responseDto);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCustomer(long id)
        {
            CustomerResponseDto responseDto = await _customerService.GetCustomer(id);

            return Ok(responseDto);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterUserRequestDto requestDto)
        {
            CustomerResponseDto responseDto = await _customerService.RegisterCustomers(requestDto);

            return Ok(responseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateCustomer(long id, [FromBody] UpdateUserRequestDto requestDto)
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            if (userId != id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseDto()
                {
                    Message = "Users can't update information of other users. Access is restricted."
                });
            }

            CustomerResponseDto responseDto = await _customerService.UpdateCustomers(id, requestDto);

            return Ok(responseDto);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(long id)
        {
            DeleteEntityResponseDto responseDto = await _customerService.DeleteCustomers(id);

            return Ok(responseDto);
        }
    }
}
