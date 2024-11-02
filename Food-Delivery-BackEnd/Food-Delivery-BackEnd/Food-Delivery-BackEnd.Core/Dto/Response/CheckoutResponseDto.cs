namespace Food_Delivery_BackEnd.Core.Dto.Response
{
    public class CheckoutResponseDto
    {
        public OrderResponseDto Order { get; set; } = default!;
        public string SessionUrl { get; set; } = string.Empty;
    }
}
