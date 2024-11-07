using AutoMapper;
using Food_Delivery_BackEnd.Core.Dto.Request;
using Food_Delivery_BackEnd.Core.Dto.Response;
using Food_Delivery_BackEnd.Core.Exceptions;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Food_Delivery_BackEnd.Data.Context;
using Food_Delivery_BackEnd.Data.Enums;
using Food_Delivery_BackEnd.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stripe.Checkout;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Stripe.TestHelpers;

namespace Food_Delivery_BackEnd.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly FoodDeliveryDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        //private readonly IConfigurationSection _clientSettings;

        public OrderService(IMapper mapper, FoodDeliveryDbContext dbContext, IStoreService storeService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _storeService = storeService;
            //_clientSettings = clientSettings.GetSection("ClientSettings");
        }

        public async Task<List<Order>> GetAllOrders()
        {
            return await _dbContext.Orders.Include(x => x.Store).Include(x => x.Items).ThenInclude(x => x.Product).ToListAsync();
        }

        public async Task<Order?> GetOrderById(long id)
        {
            return await _dbContext.Orders.Include(x => x.Store).Include(x => x.Items).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Order>> GetOrdersByCustomer(long customerId)
        {
            return await _dbContext.Orders.Include(x => x.Store).Include(x => x.Items).ThenInclude(x => x.Product).Where(x => x.CustomerId == customerId).ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByPartner(long partnerId)
        {
            return await _dbContext.Orders.Include(x => x.Store).Include(x => x.Items).ThenInclude(x => x.Product).Where(x => x.Store.PartnerId == partnerId).ToListAsync();
        }

        public async Task<Order> CreateOrder(Order order)
        {
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            _dbContext.Entry(order).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return order;
        }

        public async Task<List<OrderResponseDto>> GetOrders(long userId, UserType userType)
        {
            List<Order> orders = new List<Order>();

            switch (userType)
            {
                case UserType.Customer:
                    orders = await GetOrdersByCustomer(userId);
                    break;
                case UserType.Partner:
                    orders = await GetOrdersByPartner(userId);
                    break;
                case UserType.Admin:
                    orders = await GetAllOrders();
                    break;
            }

            return _mapper.Map<List<OrderResponseDto>>(orders);
        }

        public async Task<CheckoutResponseDto> CreateCheckout(long customerId, OrderRequestDto requestDto)
        {
            Order order = _mapper.Map<Order>(requestDto);
            order.CustomerId = customerId;

            if (order.Items == null || !order.Items.Any())
            {
                throw new ValidationException("Order must contain at least one item.");
            }

            if (order.StoreId <= 0)
            {
                throw new ValidationException("Invalid store ID.");
            }

            // Validate each order item
            foreach (var orderItem in order.Items)
            {
                if (orderItem.Quantity <= 0)
                {
                    throw new ValidationException($"Quantity for product ID {orderItem.ProductId} must be greater than zero.");
                }
            }
            //Point deliveryLocationPoint = _mapper.Map<Point>(order.Coordinate);

            //if (!deliveryLocationPoint.IsValid)
            //{
            //    throw new InvalidTopologyException("Delivery location is not a valid location");
            //}

            //deliveryLocationPoint.SRID = 4326;
            //order.DeliveryLocation = deliveryLocationPoint;

            Store? store = await _storeService.GetStoreById(order.StoreId);

            if (store == null)
            {
                throw new ResourceNotFoundException("Store with this id doesn't exist");
            }

            order.Store = store;

            //if (!deliveryLocationPoint.Within(store.DeliveryArea))
            //{
            //    throw new AddressNotSupportedException("This store doesn't deliver to your location.");
            //}

            foreach (OrderItem orderItem in order.Items)
            {
                Product? product = await _productService.GetProductById(orderItem.ProductId);

                if (product == null)
                {
                    // Should it throw exception and stop the order or just ignore this order item?
                    throw new ResourceNotFoundException($"Product with this id ({orderItem.ProductId}) doesn't exist");
                }

                if (product.StoreId != store.Id)
                {
                    throw new IncompatibleItemsError("All items in one order must be from the same store");
                }

                if (product.Quantity < orderItem.Quantity)
                {
                    throw new InsufficientQuantityException($"Not enough products available. Available quantity: {product.Quantity}");
                }

                // Save product's current information
                orderItem.ProductName = product.Name;
                orderItem.ProductPrice = product.Price;
                orderItem.ProductImage = product.Image;
                orderItem.ProductDescription = product.Description;

                orderItem.TotalPrice = orderItem.Quantity * product.Price;
            }

            order.ItemsPrice = order.Items.Aggregate(0m, (total, item) => total + item.TotalPrice);
            order.DeliveryFee = store.DeliveryFee;
            order.TotalPrice = order.ItemsPrice + order.DeliveryFee;
            order.CreatedAt = DateTime.UtcNow;

            List<SessionLineItemOptions> lineItems = order.Items.Select(item =>
            {
                List<string> lineItemImages = new List<string>();

                if (item.ProductImage != null)
                {
                    lineItemImages.Add(item.ProductImage);
                }

                return new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = item.ProductName,
                            Description = item.ProductDescription,
                            Images = lineItemImages,
                            Metadata = new Dictionary<string, string>()
                            {
                                { "ProductId", item.ProductId.ToString() },
                                { "Quantity", item.Quantity.ToString() }
                            }
                        },
                        UnitAmountDecimal = item.TotalPrice * 100,
                        Currency = "usd"
                    },
                    Quantity = 1
                };
            }).ToList();

            //var clientDomain = _clientSettings.GetValue<string>("ClientDomain");

            var options = new SessionCreateOptions()
            {
                LineItems = lineItems,
                Metadata = new Dictionary<string, string>()
                {
                    { "CustomerId", order.CustomerId.ToString() },
                    { "StoreId", order.StoreId.ToString() },
                    { "Address", order.Address }
                    //{ "Coordinate", $"{order.Coordinate.X};{order.Coordinate.Y}" },
                },
                Mode = "payment",
                //SuccessUrl = clientDomain + "/payment?status=success",
                //CancelUrl = clientDomain + "/payment?status=cancel"
            };

            var service = new SessionService();

            Session session = service.Create(options);

            return new CheckoutResponseDto()
            {
                Order = _mapper.Map<OrderResponseDto>(order),
                SessionUrl = session.Url
            };
        }

        public async Task<OrderResponseDto> CreateOrder(long customerId, OrderRequestDto requestDto)
        {
            Order order = _mapper.Map<Order>(requestDto);
            order.CustomerId = customerId;

            if (order.Items == null || !order.Items.Any())
            {
                throw new ValidationException("Order must contain at least one item.");
            }

            if (order.StoreId <= 0)
            {
                throw new ValidationException("Invalid store ID.");
            }

            // Validate each order item
            foreach (var orderItem in order.Items)
            {
                if (orderItem.Quantity <= 0)
                {
                    throw new ValidationException($"Quantity for product ID {orderItem.ProductId} must be greater than zero.");
                }
            }
            Point deliveryLocationPoint = _mapper.Map<Point>(order.Coordinate);

            //if (!deliveryLocationPoint.IsValid)
            //{
            //    throw new InvalidTopologyException("Delivery location is not a valid location");
            //}

            //deliveryLocationPoint.SRID = 4326;
            //order.DeliveryLocation = deliveryLocationPoint;

            Store? store = await _storeService.GetStoreById(order.StoreId);

            if (store == null)
            {
                throw new ResourceNotFoundException("Store with this id doesn't exist");
            }

            //if (!deliveryLocationPoint.Within(store.DeliveryArea))
            //{
            //    throw new AddressNotSupportedException("This store doesn't deliver to your location.");
            //}

            foreach (OrderItem orderItem in order.Items)
            {
                Product? product = await _productService.GetProductById(orderItem.ProductId);

                if (product == null)
                {
                    // Should it throw exception and stop the order or just ignore this order item?
                    throw new ResourceNotFoundException($"Product with this id ({orderItem.ProductId}) doesn't exist");
                }

                if (product.StoreId != store.Id)
                {
                    throw new IncompatibleItemsError("All items in one order must be from the same store");
                }

                if (product.Quantity < orderItem.Quantity)
                {
                    throw new InsufficientQuantityException($"Not enough products available. Available quantity: {product.Quantity}");
                }

                // Save product's current information
                orderItem.ProductName = product.Name;
                orderItem.ProductPrice = product.Price;
                orderItem.ProductImage = product.Image;

                orderItem.TotalPrice = orderItem.Quantity * product.Price;
                product.Quantity -= orderItem.Quantity;
            }

            order.ItemsPrice = order.Items.Aggregate(0m, (total, item) => total + item.TotalPrice);
            order.DeliveryFee = store.DeliveryFee;
            order.TotalPrice = order.ItemsPrice + order.DeliveryFee;
            order.CreatedAt = DateTime.UtcNow;

            order = await CreateOrder(order);

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<DeleteEntityResponseDto> RefundOrder(long orderId, long customerId)
        {
            Order? order = await GetOrderById(orderId);

            if (order == null)
            {
                throw new ResourceNotFoundException("Order with this id doesn't exist");
            }

            if (order.CustomerId != customerId)
            {
                throw new ActionNotAllowedException("Unauthorized to cancel this order. Only the creator can perform this action.");
            }

            DateTime deliveryTime = order.CreatedAt.AddMinutes((int)order.Store.DeliveryTimeInMinutes);

            if (DateTime.UtcNow > deliveryTime)
            {
                throw new OrderCancellationException("Cannot cancel the order because it has already been completed.");
            }

            var options = new Stripe.RefundCreateOptions()
            {
                PaymentIntent = order.PaymentIntentId,
                Metadata = new Dictionary<string, string>()
                {
                    { "CustomerId", order.CustomerId.ToString() },
                    { "OrderId", order.Id.ToString() },
                }
            };

            var service = new RefundService();

            //service.Create(options);

            return _mapper.Map<DeleteEntityResponseDto>(order);
        }

        public async Task<DeleteEntityResponseDto> CancelOrder(long orderId, long customerId)
        {
            Order? order = await GetOrderById(orderId);

            if (order == null)
            {
                throw new ResourceNotFoundException("Order with this id doesn't exist");
            }

            if (order.CustomerId != customerId)
            {
                throw new ActionNotAllowedException("Unauthorized to cancel this order. Only the creator can perform this action.");
            }

            DateTime deliveryTime = order.CreatedAt.AddMinutes((int)order.Store.DeliveryTimeInMinutes);

            if (DateTime.UtcNow > deliveryTime)
            {
                throw new OrderCancellationException("Cannot cancel the order because it has already been completed.");
            }

            order.IsCanceled = true;
            order.Items.ForEach(item =>
            {
                item.Product.Quantity += item.Quantity;
            });

            await UpdateOrder(order);

            return _mapper.Map<DeleteEntityResponseDto>(order);
        }
    }
}
