using Food_Delivery_BackEnd.Data.Enums;

namespace Food_Delivery_BackEnd.Core.Dto.Response
{
    public class PartnerResponseDto : UserResponseDto
    {
        public PartnerStatus Status { get; set; }
    }
}
