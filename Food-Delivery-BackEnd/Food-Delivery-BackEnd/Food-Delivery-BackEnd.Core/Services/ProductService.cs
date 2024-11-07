using AutoMapper;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Core.Exceptions;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Food_Delivery_BackEnd.Data.Context;
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
    public class ProductService : IProductService
    {
        private readonly FoodDeliveryDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IStoreService _storeService;

        public ProductService(FoodDeliveryDbContext dbContext, IMapper mapper, IStoreService storeService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _storeService = storeService;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            return await _dbContext.Products.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<List<Product>> GetProductsByStore(long storeId)
        {
            return await _dbContext.Products.Where(x => x.StoreId == storeId && !x.IsDeleted).ToListAsync();
        }

        public async Task<Product?> GetProductById(long id)
        {
            return await _dbContext.Products.Include(x => x.Store).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<Product> CreateProduct(Product product)
        {
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProduct(Product product)
        {
            _dbContext.Entry(product).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task DeleteProduct(Product product)
        {
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return;
        }

        public async Task<List<ProductResponseDto>> GetProducts(long? storeId)
        {
            List<Product> products;

            if (storeId == null)
            {
                products = await GetAllProducts();
            }
            else
            {
                products = await GetProductsByStore(storeId.Value);
            }

            return _mapper.Map<List<ProductResponseDto>>(products);
        }

        public async Task<ProductResponseDto> GetProduct(long id)
        {
            Product? product = await GetProductById(id);

            if (product == null)
            {
                throw new ResourceNotFoundException("Product with this id doesn't exist");
            }

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<ProductResponseDto> CreateProducts(long partnerId, CreateProductRequestDto requestDto)
        {
            Product product = _mapper.Map<Product>(requestDto);

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ValidationException("Product name cannot be empty.");
            }

            if (product.Price <= 0)
            {
                throw new ValidationException("Product price must be greater than zero.");
            }

            if (product.Quantity < 0)
            {
                throw new ValidationException("Product stock cannot be negative.");
            }

            // Check if the associated store exists
            Store? store = await _storeService.GetStoreById(product.StoreId);

            if (store == null)
            {
                throw new ResourceNotFoundException("Store with this id doesn't exist");
            }

            // Check if the partner is authorized to add products to the store
            if (store.PartnerId != partnerId)
            {
                throw new ActionNotAllowedException("Unauthorized to add products to this store. Only the creator can perform this action.");
            }


            product = await CreateProduct(product);

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<ProductResponseDto> UpdateProducts(long id, long partnerId, UpdateProductRequestDto requestDto)
        {
            Product? product = await GetProductById(id);

            if (product == null)
            {
                throw new ResourceNotFoundException("Product with this id doesn't exist");
            }

            // Check if the partner is authorized to update the product
            if (product.Store.PartnerId != partnerId)
            {
                throw new ActionNotAllowedException("Unauthorized to update this product. Only the creator can perform this action.");
            }

            // Map the request DTO to the updated product entity
            Product updatedProduct = _mapper.Map<Product>(requestDto);

            // Custom validation logic
            if (string.IsNullOrWhiteSpace(updatedProduct.Name))
            {
                throw new ValidationException("Product name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedProduct.Description))
            {
                throw new ValidationException("Product description cannot be empty.");
            }

            if (updatedProduct.Price <= 0)
            {
                throw new ValidationException("Product price must be greater than zero.");
            }

            if (updatedProduct.Quantity < 0)
            {
                throw new ValidationException("Product quantity cannot be negative.");
            }

            // Map the updated values back to the original product object
            _mapper.Map(requestDto, product);

            // Update the product in the repository
            product = await UpdateProduct(product);

            // Map the updated product to the response DTO
            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<DeleteEntityResponseDto> DeleteProducts(long id, long partnerId)
        {
            Product? product = await GetProductById(id);

            if (product == null)
            {
                throw new ResourceNotFoundException("Product with this id doesn't exist");
            }

            if (product.Store.PartnerId != partnerId)
            {
                throw new ActionNotAllowedException("Unauthorized to delete this product. Only the creator can perform this action.");
            }

            product.IsDeleted = true;

            await UpdateProduct(product);

            return _mapper.Map<DeleteEntityResponseDto>(product);
        }

        public async Task<ImageResponseDto> UploadImage(long productId, long partnerId, Stream imageStream, string imageName)
        {
            if (imageStream == null || imageStream.Length == 0)
            {
                throw new InvalidImageException("Provided image is invalid. Please ensure that the image has valid content");
            }

            Product? existingProduct = await GetProductById(productId);

            if (existingProduct == null)
            {
                throw new ResourceNotFoundException("Store with this id doesn't exist");
            }

            if (existingProduct.Store.PartnerId != partnerId)
            {
                throw new ActionNotAllowedException("Unauthorized to update this store. Only the creator can perform this action.");
            }

            // Custom logic for image deletion
            if (existingProduct.ImagePublicId != null)
            {
                string existingImagePath = Path.Combine("path/to/images", existingProduct.ImagePublicId);
                if (File.Exists(existingImagePath))
                {
                    File.Delete(existingImagePath); // Delete the existing image
                }
            }

            // Custom logic for uploading the image
            string newImagePublicId = Guid.NewGuid().ToString(); // Generate a new unique ID for the image
            string newImagePath = Path.Combine("path/to/images", newImagePublicId);

            using (var fileStream = new FileStream(newImagePath, FileMode.Create, FileAccess.Write))
            {
                await imageStream.CopyToAsync(fileStream); // Save the image to the local file system
            }

            // Update the store with the new image information
            existingProduct.ImagePublicId = newImagePublicId;
            existingProduct.Image = $"http://yourdomain.com/images/{newImagePublicId}"; // Update with the new image URL

            existingProduct = await UpdateProduct(existingProduct);

            return _mapper.Map<ImageResponseDto>(existingProduct);
        }
    }
}
