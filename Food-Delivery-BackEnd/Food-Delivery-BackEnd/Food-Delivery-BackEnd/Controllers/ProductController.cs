using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Food_Delivery_BackEnd.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] long? storeId)
        {
            List<ProductResponseDto> responseDto = await _productService.GetProducts(storeId ?? null);

            return Ok(responseDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(long id)
        {
            ProductResponseDto responseDto = await _productService.GetProduct(id);

            return Ok(responseDto);
        }

        [HttpPost]
        [Authorize(Roles = "Partner", Policy = "VerifiedPartner")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto requestDto)
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            ProductResponseDto responseDto = await _productService.CreateProducts(userId, requestDto);

            return Ok(responseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Partner", Policy = "VerifiedPartner")]
        public async Task<IActionResult> UpdateProduct(long id, [FromBody] UpdateProductRequestDto requestDto)
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            ProductResponseDto responseDto = await _productService.UpdateProducts(id, userId, requestDto);

            return Ok(responseDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Partner", Policy = "VerifiedPartner")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            DeleteEntityResponseDto responseDto = await _productService.DeleteProducts(id, userId);

            return Ok(responseDto);
        }

        [HttpPut("{id}/image")]
        [Authorize(Roles = "Partner", Policy = "VerifiedPartner")]
        public async Task<IActionResult> UploadImage(long id, [FromForm] IFormFile image)
        {
            Claim? idClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            long userId = long.Parse(idClaim!.Value);

            using Stream imageStream = image.OpenReadStream();

            ImageResponseDto responseDto = await _productService.UploadImage(id, userId, imageStream, image.FileName);

            return Ok(responseDto);
        }
    }
}
