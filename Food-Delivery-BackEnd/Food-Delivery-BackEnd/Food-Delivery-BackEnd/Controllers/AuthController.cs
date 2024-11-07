using Food_Delivery_BackEnd.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Core.Dto;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Food_Delivery_BackEnd.Core.Dto.Request;

namespace Food_Delivery_BackEnd.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            if (idClaim == null)
            {
                return BadRequest("User ID claim not found.");
            }

            if (!long.TryParse(idClaim.Value, out long userId))
            {
                return BadRequest("Invalid User ID format.");
            }

            Claim? roleClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            UserType userType = (UserType)Enum.Parse(typeof(UserType), roleClaim!.Value);

            UserResponseDto responseDto = await _authService.GetProfile(userId, userType);

            return Ok(responseDto);
        }

        [HttpPost("token")]
        public async Task<IActionResult> GenerateToken([FromBody] CreateTokenRequestDto requestDto)
        {
            var responseDto = await _authService.GenerateToken(requestDto);

            return Ok(responseDto);
        }

        [HttpDelete("token")]
        [Authorize]
        public async Task<IActionResult> DeleteRefreshToken([FromBody] DeleteTokenRequestDto requestDto)
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            Claim? roleClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            UserType userType = (UserType)Enum.Parse(typeof(UserType), roleClaim!.Value);

            await _authService.DeleteToken(userId, userType, requestDto);

            return NoContent();
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequestDto requestDto)
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            Claim? roleClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            UserType userType = (UserType)Enum.Parse(typeof(UserType), roleClaim!.Value);

            UserResponseDto responseDto = await _authService.UpdateProfile(userId, userType, requestDto);

            return Ok(responseDto);
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto requestDto)
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            Claim? roleClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            UserType userType = (UserType)Enum.Parse(typeof(UserType), roleClaim!.Value);

            await _authService.ChangePassword(userId, userType, requestDto);

            return Ok("Password successfully changed");
        }

        [HttpPut("image")]
        [Authorize]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            Claim? roleClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            UserType userType = (UserType)Enum.Parse(typeof(UserType), roleClaim!.Value);

            using Stream imageStream = image.OpenReadStream();

            ImageResponseDto responseDto = await _authService.UploadImage(userId, userType, imageStream, image.FileName);

            return Ok(responseDto);
        }

        [HttpDelete("image")]
        [Authorize]
        public async Task<IActionResult> RemoveImage()
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            Claim? roleClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            UserType userType = (UserType)Enum.Parse(typeof(UserType), roleClaim!.Value);

            await _authService.RemoveImage(userId, userType);

            return NoContent();
        }
    }
}
