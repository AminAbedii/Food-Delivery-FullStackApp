﻿namespace Food_Delivery_BackEnd.Core.Exceptions
{
    public class AddressNotSupportedException : Exception
    {
        public AddressNotSupportedException()
        {
        }

        public AddressNotSupportedException(string? message) : base(message)
        {
        }

        public AddressNotSupportedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
