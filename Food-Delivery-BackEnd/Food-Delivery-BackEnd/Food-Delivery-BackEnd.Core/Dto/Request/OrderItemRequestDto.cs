namespace Food_Delivery_BackEnd.Core.Dto.Request
{
    public class OrderItemRequestDto
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
