namespace Food_Delivery_BackEnd.Core.Exceptions
{
    public class ActionNotAllowedException : Exception
    {
        public ActionNotAllowedException()
        {
        }

        public ActionNotAllowedException(string? message) : base(message)
        {
        }

        public ActionNotAllowedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
