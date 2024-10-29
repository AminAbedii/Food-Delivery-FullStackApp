namespace Food_Delivery_BackEnd.Core.Exceptions
{
    public class InsufficientQuantityException : Exception
    {
        public InsufficientQuantityException()
        {
        }

        public InsufficientQuantityException(string? message) : base(message)
        {
        }

        public InsufficientQuantityException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
