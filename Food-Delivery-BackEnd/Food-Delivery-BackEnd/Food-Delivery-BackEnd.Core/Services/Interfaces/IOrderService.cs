using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Data.Enums;
using Food_Delivery_BackEnd.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Core.Services.Interfaces
{
    public interface IOrderService
    {
        public Task<List<Order>> GetAllOrders();
        public Task<Order?> GetOrderById(long id);
        public Task<List<Order>> GetOrdersByCustomer(long customerId);
        public Task<List<Order>> GetOrdersByPartner(long partnerId);
        public Task<Order> CreateOrder(Order order);
        public Task<Order> UpdateOrder(Order order);



        public Task<List<OrderResponseDto>> GetOrders(long userId, UserType userType);
        public Task<CheckoutResponseDto> CreateCheckout(long customerId, OrderRequestDto requestDto);
        public Task<OrderResponseDto> CreateOrder(long customerId, OrderRequestDto requestDto);
        public Task<DeleteEntityResponseDto> RefundOrder(long orderId, long customerId);
        public Task<DeleteEntityResponseDto> CancelOrder(long orderId, long customerId);
    }
}
