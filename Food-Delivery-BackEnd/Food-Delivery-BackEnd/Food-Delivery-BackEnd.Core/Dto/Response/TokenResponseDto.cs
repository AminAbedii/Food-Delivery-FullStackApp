using Food_Delivery_BackEnd.Data.Enums;

namespace FoodDeliveryServer.Common.Dto.Response
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public long IssuedAt { get; set; }
        public int ExpiresIn { get; set; }
    }
}
