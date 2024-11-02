using Food_Delivery_BackEnd.Core.Dto.Shared;

namespace Food_Delivery_BackEnd.Core.Dto.Request
{
    public class OrderRequestDto
    {
        public long StoreId { get; set; }
        public List<OrderItemRequestDto> Items { get; set; } = default!;
        public string Address { get; set; } = string.Empty;
        public CoordinateDto Coordinate { get; set; } = default!;
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
