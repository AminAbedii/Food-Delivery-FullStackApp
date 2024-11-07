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
    public interface IProductService
    {
        public Task<List<Product>> GetAllProducts();
        public Task<List<Product>> GetProductsByStore(long storeId);
        public Task<Product?> GetProductById(long id);
        public Task<Product> CreateProduct(Product product);
        public Task<Product> UpdateProduct(Product product);
        public Task DeleteProduct(Product product);




        public Task<List<ProductResponseDto>> GetProducts(long? storeId);
        public Task<ProductResponseDto> GetProduct(long id);
        public Task<ProductResponseDto> CreateProducts(long partnerId, CreateProductRequestDto requestDto);
        public Task<ProductResponseDto> UpdateProducts(long id, long partnerId, UpdateProductRequestDto requestDto);
        public Task<DeleteEntityResponseDto> DeleteProducts(long id, long partnerId);
        public Task<ImageResponseDto> UploadImage(long productId, long partnerId, Stream imageStream, string imageName);
    }
}
