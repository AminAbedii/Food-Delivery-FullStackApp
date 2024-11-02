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
    public interface IStoreService
    {
        public Task<List<Store>> GetAllStores();
        public Task<List<Store>> GetStoresByCategory(string category);
        public Task<List<Store>> GetStoresByCity(string city);
        public Task<Store?> GetStoreById(long id);
        public Task<Store> CreateStore(Store store);
        public Task<Store> UpdateStore(Store store);
        public Task DeleteStore(Store store);




        public Task<List<StoreResponseDto>> GetStores(long? partnerId, double? latitude, double? longitude);
        public Task<StoreResponseDto> GetStore(long id);
        public Task<StoreResponseDto> CreateStores(long partnerId, StoreRequestDto requestDto);
        public Task<StoreResponseDto> UpdateStores(long id, long partnerId, StoreRequestDto requestDto);
        public Task<DeleteEntityResponseDto> DeleteStores(long id);
        public Task<ImageResponseDto> UploadImage(long storeId, long partnerId, Stream imageStream, string imageName);
    }
}
