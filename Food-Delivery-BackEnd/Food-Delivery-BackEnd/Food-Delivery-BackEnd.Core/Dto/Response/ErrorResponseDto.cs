namespace Food_Delivery_BackEnd.Core.Dto.Response
{
    public class ErrorResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
