using Food_Delivery_BackEnd.Data.Enums;

namespace Food_Delivery_BackEnd.Core.Dto.Request
{
    public class CreateTokenRequestDto
    {
        public long Id { get; set; }
        public GrantType GrantType { get; set; }
        public UserType? UserType { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? RefreshToken { get; set; }
    }
}
