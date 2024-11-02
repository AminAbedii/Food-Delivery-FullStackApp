using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Food_Delivery_BackEnd.Controllers
{
    [Route("api/partners")]
    [ApiController]
    public class PartnerController : ControllerBase
    {
        private readonly IPartnerService _partnerService;

        public PartnerController(IPartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPartners([FromQuery] string? status = null)
        {
            List<PartnerResponseDto> responseDto = await _partnerService.GetPartners(status);

            return Ok(responseDto);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPartner(long id)
        {
            PartnerResponseDto responseDto = await _partnerService.GetPartner(id);

            return Ok(responseDto);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPartner([FromBody] RegisterUserRequestDto requestDto)
        {
            PartnerResponseDto responseDto = await _partnerService.RegisterPartners(requestDto);

            return Ok(responseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Partner")]
        public async Task<IActionResult> UpdatePartner(long id, [FromBody] UpdateUserRequestDto requestDto)
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

            PartnerResponseDto responseDto = await _partnerService.UpdatePartners(id, requestDto);

            return Ok(responseDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePartner(long id)
        {
            DeleteEntityResponseDto responseDto = await _partnerService.DeletePartners(id);

            return Ok(responseDto);
        }


        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyPartner(long id, [FromBody] VerifyPartnerRequestDto requestDto)
        {
            PartnerResponseDto responseDto = await _partnerService.VerifyPartner(id, requestDto);

            return Ok(responseDto);
        }
    }
}
