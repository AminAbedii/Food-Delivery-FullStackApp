using AutoMapper;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Core.Exceptions;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Food_Delivery_BackEnd.Data.Context;
using Food_Delivery_BackEnd.Data.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Core.Services
{
    public class StoreService : IStoreService
    {
        private readonly FoodDeliveryDbContext _dbContext;
        private readonly IMapper _mapper;

        public StoreService(FoodDeliveryDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }


        public async Task<List<Store>> GetAllStores()
        {
            return await _dbContext.Stores.ToListAsync();
        }

        public async Task<List<Store>> GetStoresByCategory(string category)
        {
            List<Store> allStores = await GetAllStores();

            return allStores.Where(x => x.Category.ToLower() == category.ToLower()).ToList();
        }

        public async Task<List<Store>> GetStoresByCity(string city)
        {
            return await _dbContext.Stores.Where(x => x.City.ToLower() == city.ToLower()).ToListAsync();
        }

        public async Task<Store?> GetStoreById(long id)
        {
            return await _dbContext.Stores.FindAsync(id);
        }

        public async Task<Store> CreateStore(Store store)
        {
            await _dbContext.Stores.AddAsync(store);
            await _dbContext.SaveChangesAsync();
            return store;
        }

        public async Task<Store> UpdateStore(Store store)
        {
            _dbContext.Entry(store).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return store;
        }

        public async Task DeleteStore(Store store)
        {
            _dbContext.Stores.Remove(store);
            await _dbContext.SaveChangesAsync();
            return;
        }

        public async Task<List<StoreResponseDto>> GetStores(long? partnerId, double? latitude, double? longitude)
        {
            List<Store> allStores = await GetAllStores();
            List<Store> stores = new List<Store>();

            if (partnerId.HasValue)
            {
                stores = allStores.Where(x => x.PartnerId == partnerId.Value).ToList();
            }
            //else if (latitude.HasValue && longitude.HasValue)
            //{
            //    Point point = new Point(new Coordinate(longitude.Value, latitude.Value));
            //    //stores = allStores.Where(x => point.Within(x.)).ToList();
            //}
            else
            {
                stores = await GetAllStores();
            }

            return _mapper.Map<List<StoreResponseDto>>(stores);
        }

        public async Task<StoreResponseDto> GetStore(long id)
        {
            Store? store = await GetStoreById(id);

            if (store == null)
            {
                throw new ResourceNotFoundException("Store with this id doesn't exist");
            }

            return _mapper.Map<StoreResponseDto>(store);
        }

        public async Task<StoreResponseDto> CreateStores(long partnerId, StoreRequestDto requestDto)
        {
            Store store = _mapper.Map<Store>(requestDto);
            store.PartnerId = partnerId;

            if (string.IsNullOrWhiteSpace(store.Name))
            {
                throw new ValidationException("Store name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(store.Description))
            {
                throw new ValidationException("Store description cannot be empty.");
            }

            if (store.Coordinates == null)
            {
                throw new ValidationException("Store coordinates cannot be null or empty.");
            }

            //Polygon deliveryAreaPolygon = _mapper.Map<Polygon>(store.Coordinates);

            //if (!deliveryAreaPolygon.IsValid)
            //{
            //    throw new InvalidTopologyException("Delivery area is not a valid polygon");
            //}

            //deliveryAreaPolygon.SRID = 4326;
            //store.DeliveryArea = deliveryAreaPolygon;

            store = await CreateStore(store);

            return _mapper.Map<StoreResponseDto>(store);
        }

        public async Task<StoreResponseDto> UpdateStores(long id, long partnerId, StoreRequestDto requestDto)
        {
            Store? store = await GetStoreById(id);

            if (store == null)
            {
                throw new ResourceNotFoundException("Store with this id doesn't exist");
            }

            if (store.PartnerId != partnerId)
            {
                throw new ActionNotAllowedException("Unauthorized to update this store. Only the creator can perform this action.");
            }

            Store updatedStore = _mapper.Map<Store>(requestDto);

            // Custom validation logic
            if (string.IsNullOrWhiteSpace(updatedStore.Name))
            {
                throw new ValidationException("Store name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedStore.Description))
            {
                throw new ValidationException("Store description cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedStore.Address))
            {
                throw new ValidationException("Store address cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedStore.City))
            {
                throw new ValidationException("Store city cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedStore.PostalCode))
            {
                throw new ValidationException("Store postal code cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(updatedStore.Phone))
            {
                throw new ValidationException("Store phone cannot be empty.");
            }

            if (updatedStore.Coordinates == null)
            {
                throw new ValidationException("Store coordinates cannot be null or empty.");
            }

            //Polygon deliveryAreaPolygon = _mapper.Map<Polygon>(updatedStore.Coordinates);

            //if (!deliveryAreaPolygon.IsValid)
            //{
            //    throw new InvalidTopologyException("Delivery area is not a valid polygon");
            //}

            //deliveryAreaPolygon.SRID = 4326;

            // Map the updated values back to the original store object
            _mapper.Map(requestDto, store);
            //store.DeliveryArea = deliveryAreaPolygon;

            store = await UpdateStore(store);

            return _mapper.Map<StoreResponseDto>(store);
        }

        public async Task<DeleteEntityResponseDto> DeleteStores(long id)
        {
            Store? store = await GetStoreById(id);

            if (store == null)
            {
                throw new ResourceNotFoundException("Store with this id doesn't exist");
            }

            await DeleteStore(store);

            return _mapper.Map<DeleteEntityResponseDto>(store);
        }

        public async Task<ImageResponseDto> UploadImage(long storeId, long partnerId, Stream imageStream, string imageName)
        {
            if (imageStream == null || imageStream.Length == 0)
            {
                throw new InvalidImageException("Provided image is invalid. Please ensure that the image has valid content");
            }

            Store? existingStore = await GetStoreById(storeId);

            if (existingStore == null)
            {
                throw new ResourceNotFoundException("Store with this id doesn't exist");
            }

            if (existingStore.PartnerId != partnerId)
            {
                throw new ActionNotAllowedException("Unauthorized to update this store. Only the creator can perform this action.");
            }

            // Custom logic for image deletion
            if (existingStore.ImagePublicId != null)
            {
                string existingImagePath = Path.Combine("path/to/images", existingStore.ImagePublicId);
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
            existingStore.ImagePublicId = newImagePublicId;
            existingStore.Image = $"http://yourdomain.com/images/{newImagePublicId}"; // Update with the new image URL

            existingStore = await UpdateStore(existingStore);

            return _mapper.Map<ImageResponseDto>(existingStore);
        }
    }
}
